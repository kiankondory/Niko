// ============================================================================
// Niko.Core — CoachUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: اجرای policy، ترجیحات و fallback محلی مربی در یک مرز application.
// وابستگی‌ها و لایه: UseCases/Coach → ICoachPreferencesStore و Domain/Coach.
// نکات تغییر و قیود: پیش‌فرض خاموش است؛ در این مرحله هیچ provider خارجی، API key یا
//           backend وجود ندارد. خطا به fallback امن تبدیل می‌شود، نه متن خام.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;

namespace Niko.Core.UseCases.Coach;

public sealed class CoachUseCase
{
    private readonly ICoachPreferencesStore _preferencesStore;

    public CoachUseCase(ICoachPreferencesStore preferencesStore)
    {
        _preferencesStore = preferencesStore;
    }

    public async Task<CoachPreferences> GetPreferencesAsync(CancellationToken ct = default)
        => await _preferencesStore.GetAsync(ct).ConfigureAwait(false) ?? new CoachPreferences();

    public Task SetPreferencesAsync(CoachPreferences preferences, CancellationToken ct = default)
        => _preferencesStore.SaveAsync(preferences, ct);

    public Task ClearCoachDataAsync(CancellationToken ct = default)
        => _preferencesStore.ClearAsync(ct);

    public async Task<CoachResponse> GenerateAsync(CoachRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var preferences = await GetPreferencesAsync(ct).ConfigureAwait(false);
        if (!preferences.Enabled)
        {
            return new(false, CoachSafetyStatus.Disabled,
                CoachProviderResult.Failure(CoachProviderError.Disabled), Array.Empty<CoachSuggestion>());
        }

        if (!CoachPolicy.IsContextAllowed(request.Context))
        {
            return new(true, CoachSafetyStatus.Rejected,
                CoachProviderResult.Failure(CoachProviderError.PolicyViolation), Array.Empty<CoachSuggestion>());
        }

        if (request.Context.IsEmpty)
        {
            return new(true, CoachSafetyStatus.EmptyContext,
                CoachProviderResult.Failure(CoachProviderError.EmptyContext), Array.Empty<CoachSuggestion>());
        }

        var filtered = request.Context with
        {
            CravingIntensity = preferences.AllowCravingContext ? request.Context.CravingIntensity : null,
            ProgressPercent = preferences.AllowAggregatedProgressContext ? request.Context.ProgressPercent : null,
        };
        var result = LocalDeterministicCoach.Generate(request with { Context = filtered, AllowExternalProvider = false });
        var status = result.Succeeded
            ? CoachSafetyStatus.Safe
            : result.Error == CoachProviderError.EmptyContext
                ? CoachSafetyStatus.EmptyContext
                : CoachSafetyStatus.Rejected;
        return new(true, status,
            result, result.Suggestions);
    }
}
