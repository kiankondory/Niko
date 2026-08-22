// ============================================================================
// نام فایل: CoachProxyService.cs
// مسئولیت: اعمال consent، redaction، safety policy و fallback برای endpoint backend.
// وابستگی‌ها و لایه: Service در Backend؛ Core policy و GeminiApiClient را orchestration می‌کند.
// نکات تغییر و قیود: endpoint هیچ دادهٔ خامی را قبول نمی‌کند و هر شکست پاسخ safe می‌دهد.
// ============================================================================

using Niko.CoachProxy.Contracts;
using Niko.CoachProxy.Configuration;
using Niko.Core.Domain.Coach;

namespace Niko.CoachProxy.Services;

public sealed class CoachProxyService
{
    private readonly GeminiApiClient _client;
    private readonly GeminiOptionsAccessor _options;

    public CoachProxyService(GeminiApiClient client, GeminiOptionsAccessor options)
    {
        _client = client;
        _options = options;
    }

    public async Task<CoachProxyResponse> GenerateAsync(CoachProxyRequest request, CancellationToken ct)
    {
        if (!CoachPolicy.IsContextAllowed(new CoachContext(
                request.Context.CravingIntensity,
                request.Context.ProgressPercent,
                request.Context.SelectedIntervention,
                request.Context.MilestoneStatus,
                request.Context.UserPreferences)))
        {
            return new(false, ExternalCoachError.PayloadTooLarge, null, ExternalCoachSafetyResult.Rejected);
        }

        var bounded = request with
        {
            MaxResponseCharacters = Math.Clamp(request.MaxResponseCharacters, 1, _options.Value.MaxResponseCharacters),
            Context = request.Context with
            {
                UserPreferences = request.Context.UserPreferences.Take(8).ToArray(),
            },
        };
        var result = await _client.GenerateAsync(bounded, ct).ConfigureAwait(false);
        if (!result.Succeeded || result.Text is null)
        {
            return result;
        }

        return CoachPolicy.IsProviderTextAllowed(result.Text) && result.Text.Length <= bounded.MaxResponseCharacters
            ? result
            : new(false, ExternalCoachError.UnsafeOutput, null, ExternalCoachSafetyResult.Rejected);
    }
}

public sealed class GeminiOptionsAccessor
{
    public GeminiOptionsAccessor(Niko.CoachProxy.Configuration.GeminiOptions value) => Value = value;
    public Niko.CoachProxy.Configuration.GeminiOptions Value { get; }
}
