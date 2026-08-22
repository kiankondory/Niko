// ============================================================================
// Niko.Core — TriggerInsight.cs
// ----------------------------------------------------------------------------
// مسئولیت: یک بینش تلفیقی و ناشناس. فقط دادهٔ تجمیعی/تقریبی را حمل می‌کند و هرگز
//           جزئیات خام رویداد (متن، زمان دقیق، زمینهٔ حساس) را نمایش نمی‌دهد.
// وابستگی‌ها و لایه: بخش Domain/TriggerAnalysis در Core.
// نکات تغییر و قیود: LabelKey یک کلید محلی‌سازی است. Strength تقریبی (۰ تا ۱۰۰) است.
// ============================================================================

namespace Niko.Core.Domain.TriggerAnalysis;

/// <summary>
/// نوع یک بینش تحلیل محرک.
/// </summary>
public enum TriggerInsightKind
{
    /// <summary>الگوی زمان روز.</summary>
    TimeOfDay = 0,

    /// <summary>الگوی روز هفته.</summary>
    DayOfWeek = 1,

    /// <summary>الگوی زمینه (دستهٔ کاربر).</summary>
    Context = 2,

    /// <summary>فراوانی هوس.</summary>
    CravingFrequency = 3,

    /// <summary>مقایسهٔ مصرف در برابر مقاومت.</summary>
    SmokedVsResisted = 4,
}

/// <summary>
/// بینش تلفیقی.
/// </summary>
public sealed record TriggerInsight(
    TriggerInsightKind Kind,
    string LabelKey,
    double Strength,
    int Count,
    bool IsApproximate)
{
    /// <summary>پارامترهای قالب‌بندی متن (برای کلیدهای دارای جای‌نگهدار).</summary>
    public IReadOnlyDictionary<string, object>? Args { get; init; }
}
