// ============================================================================
// نام فایل: CoachProxyContracts.cs
// مسئولیت: قرارداد HTTP داخلی proxy برای زمینهٔ تجمیعی و پاسخ امن مربی.
// وابستگی‌ها و لایه: Contracts در Backend؛ از ApprovedCoachContext در Core استفاده می‌کند.
// نکات تغییر و قیود: هیچ event خام، note، شناسه، timestamp یا metadata خصوصی پذیرفته نمی‌شود.
// ============================================================================

using Niko.Core.Domain.Coach;

namespace Niko.CoachProxy.Contracts;

public sealed record CoachProxyRequest(
    ApprovedCoachContext Context,
    int MaxResponseCharacters = 500);

public sealed record CoachProxyResponse(
    bool Succeeded,
    ExternalCoachError Error,
    string? Text,
    ExternalCoachSafetyResult SafetyResult);
