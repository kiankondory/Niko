// ============================================================================
// نام فایل: BackendCoachProxyProvider.cs
// مسئولیت: ارسال درخواست provider خارجی از موبایل فقط به proxy امن backend.
// وابستگی‌ها و لایه: Infrastructure adapter → Core IExternalCoachProvider؛ بدون Gemini SDK یا API key.
// نکات تغییر و قیود: تنظیمات runtime اختیاری است؛ در نبود endpoint/token، provider unavailable و fallback محلی است.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;

namespace Niko.Infrastructure.Coach;

public sealed class BackendCoachProxyProvider : IExternalCoachProvider
{
    private readonly HttpClient _httpClient;
    private readonly Uri? _endpoint;
    private readonly Uri? _healthEndpoint;
    private readonly string _token;

    public BackendCoachProxyProvider(HttpClient httpClient, string? endpoint, string? healthEndpoint, string? sessionToken)
    {
        _httpClient = httpClient;
        _endpoint = Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri
            : null;
        _healthEndpoint = Uri.TryCreate(healthEndpoint, UriKind.Absolute, out var healthUri) && healthUri.Scheme == Uri.UriSchemeHttps
            ? healthUri
            : null;
        _token = sessionToken ?? string.Empty;
    }

    public async Task<ExternalCoachAvailability> GetAvailabilityAsync(CancellationToken ct = default)
    {
        if (_healthEndpoint is null || string.IsNullOrWhiteSpace(_token))
        {
            return ExternalCoachAvailability.FailClosed(
                string.IsNullOrWhiteSpace(_token)
                    ? ExternalCoachAvailabilityState.AuthenticationRequired
                    : ExternalCoachAvailabilityState.NotConfigured);
        }

        using var message = new HttpRequestMessage(HttpMethod.Get, _healthEndpoint);
        message.Headers.Add("X-Coach-Session", _token);
        try
        {
            using var response = await _httpClient.SendAsync(message, ct).ConfigureAwait(false);
            var status = await response.Content.ReadFromJsonAsync<AvailabilityResponse>(cancellationToken: ct).ConfigureAwait(false);
            if (status is null)
            {
                return ExternalCoachAvailability.FailClosed(ExternalCoachAvailabilityState.Unavailable);
            }

            return new(status.State, status.IsFree, status.BillingDisabled, status.HasPaidFallback);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return ExternalCoachAvailability.FailClosed(ExternalCoachAvailabilityState.Unavailable);
        }
        catch (HttpRequestException)
        {
            return ExternalCoachAvailability.FailClosed(ExternalCoachAvailabilityState.Unavailable);
        }
        catch (System.Text.Json.JsonException)
        {
            return ExternalCoachAvailability.FailClosed(ExternalCoachAvailabilityState.Unavailable);
        }
    }

    public async Task<ExternalCoachResult> GenerateAsync(ExternalCoachRequest request, CancellationToken ct = default)
    {
        var fallback = CoachProviderResult.Failure(CoachProviderError.Unavailable);
        if (_endpoint is null || string.IsNullOrWhiteSpace(_token))
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Unavailable, fallback);
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        message.Headers.Add("X-Coach-Session", _token);
        message.Content = JsonContent.Create(new
        {
            request.Context,
            request.MaxResponseCharacters,
        });

        try
        {
            using var response = await _httpClient.SendAsync(message, ct).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return ExternalCoachResult.Failure(ExternalCoachError.RateLimited, fallback);
            }

            if (!response.IsSuccessStatusCode)
            {
                return ExternalCoachResult.Failure(ExternalCoachError.ProviderFailure, fallback);
            }

            var result = await response.Content.ReadFromJsonAsync<ProxyResponse>(cancellationToken: ct).ConfigureAwait(false);
            if (result is null)
            {
                return ExternalCoachResult.Failure(ExternalCoachError.ProviderFailure, fallback);
            }

            return new(
                result.Succeeded,
                result.Error,
                result.Text is null ? null : new ExternalCoachResponse(result.Text, result.SafetyResult),
                fallback);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Timeout, fallback);
        }
        catch (HttpRequestException)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Unavailable, fallback);
        }
        catch (System.Text.Json.JsonException)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.ProviderFailure, fallback);
        }
    }

    private sealed record ProxyResponse(
        bool Succeeded,
        ExternalCoachError Error,
        string? Text,
        ExternalCoachSafetyResult SafetyResult);

    private sealed record AvailabilityResponse(
        ExternalCoachAvailabilityState State,
        bool IsFree,
        bool BillingDisabled,
        bool HasPaidFallback);
}
