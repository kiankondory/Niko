// ============================================================================
// Niko.Core — CompanionProgressSummary.cs
// ----------------------------------------------------------------------------
// مسئولیت: خلاصهٔ پیشرفت برای نمایش در ابزارک/ساعت. فقط دادهٔ نمایشی است و از
//           محاسبات هسته (ProgressCalculator) می‌آید.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core.
// نکات تغییر و قیود: هیچ منطق دامنه‌ای در ابزارک/ساعت نیست؛ این فقط خروجی است.
// ============================================================================

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// خلاصهٔ پیشرفت.
/// </summary>
public sealed record CompanionProgressSummary(
    int TotalSmoked,
    int TotalResisted,
    int TotalCravings,
    double MilestoneProgressPercent,
    bool HasSavings)
{
    /// <summary>رویدادهای معتبر مصرف در روز محلی جاری.</summary>
    public int SmokedToday { get; init; }

    /// <summary>رویدادهای معتبر مقاومت در روز محلی جاری.</summary>
    public int ResistedToday { get; init; }

    /// <summary>رویدادهای معتبر هوس در روز محلی جاری.</summary>
    public int CravingsToday { get; init; }
}
