// ============================================================================
// Niko.Core — CoachContracts.cs
// ----------------------------------------------------------------------------
// مسئولیت: قراردادهای مستقل از پلتفرم برای درخواست، زمینهٔ محدود، پاسخ، پیشنهاد
//           و وضعیت ایمنی مربی. این فایل هیچ اتصال بیرونی یا متن خام رویداد ندارد.
// وابستگی‌ها و لایه: Domain/Coach در Core؛ توسط UseCase و آداپترهای UI مصرف می‌شود.
// نکات تغییر و قیود: زمینه فقط دادهٔ تجمیعی است؛ تشخیص پزشکی، تجویز، پیش‌بینی قطعی
//           و نمایش یادداشت خصوصی ممنوع است.
// ============================================================================

namespace Niko.Core.Domain.Coach;

public enum CoachSafetyStatus
{
    Safe = 0,
    Disabled = 1,
    EmptyContext = 2,
    Rejected = 3,
}

public enum CoachProviderError
{
    None = 0,
    Disabled = 1,
    EmptyContext = 2,
    Timeout = 3,
    Unavailable = 4,
    PolicyViolation = 5,
}

public enum CoachSuggestionKind
{
    CravingSupport = 0,
    ProgressEncouragement = 1,
    MilestoneEncouragement = 2,
}

public sealed record CoachContext(
    int? CravingIntensity,
    int? ProgressPercent,
    string? SelectedIntervention,
    string? MilestoneStatus,
    IReadOnlyList<string> UserPreferences)
{
    public static CoachContext Empty { get; } = new(null, null, null, null, Array.Empty<string>());

    public bool IsEmpty =>
        CravingIntensity is null &&
        ProgressPercent is null &&
        string.IsNullOrWhiteSpace(SelectedIntervention) &&
        string.IsNullOrWhiteSpace(MilestoneStatus) &&
        UserPreferences.Count == 0;
}

public sealed record CoachRequest(
    CoachContext Context,
    bool AllowExternalProvider,
    TimeSpan Timeout)
{
    public static CoachRequest Local(CoachContext context)
        => new(context, false, TimeSpan.FromSeconds(5));
}

public sealed record CoachSuggestion(
    string Id,
    string TextKey,
    CoachSuggestionKind Kind);

public sealed record CoachProviderResult(
    bool Succeeded,
    CoachProviderError Error,
    IReadOnlyList<CoachSuggestion> Suggestions)
{
    public static CoachProviderResult Failure(CoachProviderError error)
        => new(false, error, Array.Empty<CoachSuggestion>());
}

public sealed record CoachResponse(
    bool IsEnabled,
    CoachSafetyStatus SafetyStatus,
    CoachProviderResult ProviderResult,
    IReadOnlyList<CoachSuggestion> Suggestions);
