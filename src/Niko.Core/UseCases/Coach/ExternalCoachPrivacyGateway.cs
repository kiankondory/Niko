// ============================================================================
// Niko.Core — ExternalCoachPrivacyGateway.cs
// ----------------------------------------------------------------------------
// مسئولیت: مرز رضایت و redaction برای provider خارجی و fallback امن محلی.
// وابستگی‌ها و لایه: UseCases/Coach → ICoachPreferencesStore و IExternalCoachProvider.
// نکات تغییر و قیود: بدون فعال‌سازی و رضایت صریح هیچ provider خوانده نمی‌شود؛ فقط
//           فیلدهای تجمیعی مجاز forward می‌شوند و هر failure به fallback محلی می‌رسد.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;

namespace Niko.Core.UseCases.Coach;

public sealed class ExternalCoachPrivacyGateway
{
    private readonly ICoachPreferencesStore _preferencesStore;
    private readonly IExternalCoachProvider _provider;
    private readonly ExternalCoachProviderConfiguration _configuration;

    public ExternalCoachPrivacyGateway(
        ICoachPreferencesStore preferencesStore,
        IExternalCoachProvider provider,
        ExternalCoachProviderConfiguration? configuration = null)
    {
        _preferencesStore = preferencesStore;
        _provider = provider;
        _configuration = configuration ?? new ExternalCoachProviderConfiguration();
    }

    public async Task<ExternalCoachResult> GenerateAsync(
        CoachRequest request,
        CancellationToken ct = default)
    {
        var preferences = await _preferencesStore.GetAsync(ct).ConfigureAwait(false)
            ?? new CoachPreferences();
        var filteredRequest = request with
        {
            Context = FilterContext(request.Context, preferences),
            AllowExternalProvider = false,
        };
        var fallback = LocalDeterministicCoach.Generate(filteredRequest);

        if (!preferences.Enabled)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Disabled, fallback);
        }

        if (!preferences.AllowExternalProvider)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.ConsentRequired, fallback);
        }

        if (!CoachPolicy.IsContextAllowed(request.Context))
        {
            return ExternalCoachResult.Failure(ExternalCoachError.PayloadTooLarge, fallback);
        }

        if (request.Context.IsEmpty)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.EmptyContext, fallback);
        }

        if (!_configuration.Enabled ||
            !_configuration.BillingExplicitlyDisabled ||
            _configuration.PaidFallbackConfigured)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Unavailable, fallback);
        }

        var availability = await _provider.GetAvailabilityAsync(ct).ConfigureAwait(false);
        if (availability.State != ExternalCoachAvailabilityState.AvailableFree ||
            !availability.IsFree ||
            !availability.BillingDisabled ||
            availability.HasPaidFallback)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Unavailable, fallback);
        }

        var approvedContext = Redact(filteredRequest.Context, preferences);
        if (approvedContext is null)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.PayloadTooLarge, fallback);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(EffectiveTimeout(request.Timeout));
        ExternalCoachResult result;
        try
        {
            result = await _provider.GenerateAsync(
                new ExternalCoachRequest(
                    approvedContext,
                    EffectiveTimeout(request.Timeout),
                    Math.Clamp(_configuration.MaxResponseCharacters, 1, 500)),
                timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Timeout, fallback);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Cancelled, fallback);
        }
        catch (TimeoutException) when (!ct.IsCancellationRequested)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.Timeout, fallback);
        }
        catch (InvalidOperationException) when (!ct.IsCancellationRequested)
        {
            return ExternalCoachResult.Failure(ExternalCoachError.ProviderFailure, fallback);
        }

        if (!result.Succeeded || result.Response is null)
        {
            return result with { LocalFallback = fallback };
        }

        var response = result.Response;
        if (response.SafetyResult != ExternalCoachSafetyResult.Allowed ||
            !CoachPolicy.IsProviderTextAllowed(response.Text) ||
            response.Text.Length > Math.Clamp(_configuration.MaxResponseCharacters, 1, 500))
        {
            return ExternalCoachResult.Failure(ExternalCoachError.UnsafeOutput, fallback);
        }

        return result;
    }

    public Task<ExternalCoachAvailability> GetAvailabilityAsync(CancellationToken ct = default)
        => _provider.GetAvailabilityAsync(ct);

    private static ApprovedCoachContext? Redact(
        CoachContext context,
        CoachPreferences preferences)
    {
        var preferencesList = context.UserPreferences.Take(8).ToArray();
        if (preferencesList.Any(value => value.Length > 40))
        {
            return null;
        }

        return new ApprovedCoachContext(
            preferences.AllowCravingContext ? context.CravingIntensity : null,
            preferences.AllowAggregatedProgressContext ? context.ProgressPercent : null,
            context.SelectedIntervention,
            context.MilestoneStatus,
            preferencesList);
    }

    private static CoachContext FilterContext(
        CoachContext context,
        CoachPreferences preferences)
        => context with
        {
            CravingIntensity = preferences.AllowCravingContext ? context.CravingIntensity : null,
            ProgressPercent = preferences.AllowAggregatedProgressContext ? context.ProgressPercent : null,
        };

    private TimeSpan EffectiveTimeout(TimeSpan requestTimeout)
        => requestTimeout > TimeSpan.Zero && requestTimeout < _configuration.EffectiveTimeout
            ? requestTimeout
            : _configuration.EffectiveTimeout;
}
