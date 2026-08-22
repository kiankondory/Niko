// ============================================================================
// نام فایل: GeminiApiClient.cs
// مسئولیت: ارسال درخواست محدود و ساختاریافته به Gemini generateContent در backend.
// وابستگی‌ها و لایه: Service در Backend؛ HttpClient و GeminiOptions، بدون وابستگی موبایل.
// نکات تغییر و قیود: فقط مدل تنظیم‌شده استفاده می‌شود؛ خطای 429 rate-limit است و fallback بیرونی است.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Niko.CoachProxy.Configuration;
using Niko.CoachProxy.Contracts;
using Niko.Core.Domain.Coach;

namespace Niko.CoachProxy.Services;

public sealed class GeminiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;

    public GeminiApiClient(HttpClient httpClient, GeminiOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<CoachProxyResponse> GenerateAsync(CoachProxyRequest request, CancellationToken ct)
    {
        if (!_options.IsConfigured)
        {
            return new(false, ExternalCoachError.Unavailable, null, ExternalCoachSafetyResult.Rejected);
        }

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/models/{Uri.EscapeDataString(_options.Model)}:generateContent";
        var payload = new
        {
            systemInstruction = new { parts = new[] { new { text = "Provide one short, supportive, non-medical cessation suggestion. Do not diagnose, prescribe, shame, judge, guarantee, or mention private data. Return plain text only." } } },
            contents = new[] { new { parts = new[] { new { text = BuildPrompt(request.Context) } } } },
            generationConfig = new { maxOutputTokens = 160, temperature = 0.2 },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_LOW_AND_ABOVE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_LOW_AND_ABOVE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_LOW_AND_ABOVE" },
            },
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint);
        message.Headers.Add("x-goog-api-key", _options.ApiKey);
        message.Content = JsonContent.Create(payload);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(message, timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new(false, ExternalCoachError.Timeout, null, ExternalCoachSafetyResult.Rejected);
        }
        catch (HttpRequestException)
        {
            return new(false, ExternalCoachError.ProviderFailure, null, ExternalCoachSafetyResult.Rejected);
        }

        await using var content = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return new(false, ExternalCoachError.RateLimited, null, ExternalCoachSafetyResult.Rejected);
        }

        if (!response.IsSuccessStatusCode)
        {
            return new(false, ExternalCoachError.ProviderFailure, null, ExternalCoachSafetyResult.Rejected);
        }

        try
        {
            using var document = await JsonDocument.ParseAsync(content, cancellationToken: ct).ConfigureAwait(false);
            var candidate = document.RootElement
                .GetProperty("candidates")[0];
            if (candidate.TryGetProperty("finishReason", out var finishReason) &&
                string.Equals(finishReason.GetString(), "SAFETY", StringComparison.OrdinalIgnoreCase))
            {
                return new(false, ExternalCoachError.UnsafeOutput, null, ExternalCoachSafetyResult.Rejected);
            }

            var text = candidate.GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return new(false, ExternalCoachError.ProviderFailure, null, ExternalCoachSafetyResult.Rejected);
            }

            return new(true, ExternalCoachError.None, text.Trim(), ExternalCoachSafetyResult.Allowed);
        }
        catch (JsonException)
        {
            return new(false, ExternalCoachError.ProviderFailure, null, ExternalCoachSafetyResult.Rejected);
        }
        catch (KeyNotFoundException)
        {
            return new(false, ExternalCoachError.ProviderFailure, null, ExternalCoachSafetyResult.Rejected);
        }
    }

    private static string BuildPrompt(ApprovedCoachContext context)
    {
        var values = new List<string>();
        if (context.CravingIntensity is { } craving)
        {
            values.Add($"craving_intensity={craving}");
        }

        if (context.ProgressPercent is { } progress)
        {
            values.Add($"progress_percent={progress}");
        }

        if (!string.IsNullOrWhiteSpace(context.SelectedIntervention))
        {
            values.Add($"selected_intervention={context.SelectedIntervention}");
        }

        if (!string.IsNullOrWhiteSpace(context.MilestoneStatus))
        {
            values.Add($"milestone_status={context.MilestoneStatus}");
        }

        if (context.UserPreferences.Count > 0)
        {
            values.Add($"preferences={string.Join(",", context.UserPreferences)}");
        }

        return $"Approved aggregate context only: {string.Join("; ", values)}";
    }
}
