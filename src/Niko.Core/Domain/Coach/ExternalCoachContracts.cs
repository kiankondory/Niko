// ============================================================================
// Niko.Core — ExternalCoachContracts.cs
// ----------------------------------------------------------------------------
// مسئولیت: قراردادهای provider-neutral برای درخواست خارجی، زمینهٔ تجمیعی، پاسخ،
//           وضعیت ایمنی و خطاهای timeout، rate-limit، failure و cancellation.
// وابستگی‌ها و لایه: Domain/Coach در Core؛ توسط gateway و adapterهای آینده مصرف می‌شود.
// نکات تغییر و قیود: زمینهٔ ارسالی فاقد event خام، note، شناسه، timestamp و metadata
//           خصوصی است؛ پاسخ متنی تا عبور از policy قابل نمایش نیست.
// ============================================================================

namespace Niko.Core.Domain.Coach;

public enum ExternalCoachSafetyResult
{
    Allowed = 0,
    Rejected = 1,
}

public enum ExternalCoachError
{
    None = 0,
    Disabled = 1,
    ConsentRequired = 2,
    Timeout = 3,
    RateLimited = 4,
    ProviderFailure = 5,
    Cancelled = 6,
    UnsafeOutput = 7,
    PayloadTooLarge = 8,
    Unavailable = 9,
    EmptyContext = 10,
}

public enum ExternalCoachAvailabilityState
{
    AvailableFree = 0,
    NotConfigured = 1,
    Unavailable = 2,
    FreeQuotaUnavailable = 3,
    AuthenticationRequired = 4,
    DisabledByPolicy = 5,
}

public sealed record ExternalCoachAvailability(
    ExternalCoachAvailabilityState State,
    bool IsFree,
    bool BillingDisabled,
    bool HasPaidFallback,
    string? Detail = null)
{
    public static ExternalCoachAvailability FailClosed(ExternalCoachAvailabilityState state)
        => new(state, false, false, true);
}

public sealed record ApprovedCoachContext(
    int? CravingIntensity,
    int? ProgressPercent,
    string? SelectedIntervention,
    string? MilestoneStatus,
    IReadOnlyList<string> UserPreferences);

public sealed record ExternalCoachRequest(
    ApprovedCoachContext Context,
    TimeSpan Timeout,
    int MaxResponseCharacters = 500);

public sealed record ExternalCoachResponse(
    string Text,
    ExternalCoachSafetyResult SafetyResult);

public sealed record ExternalCoachResult(
    bool Succeeded,
    ExternalCoachError Error,
    ExternalCoachResponse? Response,
    CoachProviderResult LocalFallback)
{
    public static ExternalCoachResult Failure(
        ExternalCoachError error,
        CoachProviderResult fallback)
        => new(false, error, null, fallback);
}

public sealed record ExternalCoachProviderConfiguration(
    bool Enabled = false,
    TimeSpan? Timeout = null,
    int MaxResponseCharacters = 500,
    bool BillingExplicitlyDisabled = false,
    bool PaidFallbackConfigured = true)
{
    public TimeSpan EffectiveTimeout => Timeout is { } value && value > TimeSpan.Zero
        ? value
        : TimeSpan.FromSeconds(5);
}
