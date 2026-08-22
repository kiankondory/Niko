// ============================================================================
// Niko.Core — CompanionStreakSummary.cs
// ----------------------------------------------------------------------------
// مسئولیت: خلاصهٔ استریک/میل‌استون برای نمایش در ابزارک/ساعت. فقط دادهٔ نمایشی است.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core.
// نکات تغییر و قیود: از محاسبات هسته می‌آید؛ ابزارک/ساعت منطق مستقلی ندارد.
// ============================================================================

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// خلاصهٔ استریک/میل‌استون.
/// </summary>
public sealed record CompanionStreakSummary(
    int CurrentStreakDays,
    int CurrentMilestoneDays,
    int NextMilestoneDays);
