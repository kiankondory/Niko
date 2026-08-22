// ============================================================================
// Niko.Core — ProgressSnapshot.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ مشتق‌شدهٔ داشبورد. این مدل خروجی محاسبات دامنه است و فقط دادهٔ
//           نمایشی/مشتق‌شده را در بر دارد؛ هیچ منطق محاسباتی در آن نیست.
// وابستگی‌ها و لایه: بخش Domain در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: مقادیر فقط از محاسبات خالص ساخته می‌شوند. صرفه‌جویی تقریبی
//           است و در صورت نبود ورودی کافی null می‌ماند.
// ============================================================================

namespace Niko.Core.Domain;

/// <summary>
/// نمای مشتق‌شدهٔ پیشرفت کاربر برای صفحهٔ داشبورد.
/// </summary>
public sealed record ProgressSnapshot
{
    /// <summary>مجموع رویدادهای مصرف.</summary>
    public int TotalSmoked { get; init; }

    /// <summary>مجموع رویدادهای مقاومت.</summary>
    public int TotalResisted { get; init; }

    /// <summary>تعداد هوس‌های ثبت‌شده.</summary>
    public int TotalCravings { get; init; }

    /// <summary>طول استریک فعلی بر حسب روز.</summary>
    public int CurrentStreakDays { get; init; }

    /// <summary>میل‌استون فعلی (بزرگ‌ترین آستانهٔ کمتر یا مساوی استریک).</summary>
    public int CurrentMilestoneDays { get; init; }

    /// <summary>میل‌استون بعدی.</summary>
    public int NextMilestoneDays { get; init; }

    /// <summary>درصد پیشرفت به سمت میل‌استون بعدی (۰ تا ۱۰۰).</summary>
    public double MilestoneProgressPercent { get; init; }

    /// <summary>صرفه‌جویی تقریبی (در صورت موجود بودن ورودی کافی)؛ در غیر این صورت null.</summary>
    public decimal? ApproximateSavings { get; init; }

    /// <summary>تاریخ ترک کاربر (در صورت تنظیم شده).</summary>
    public DateTimeOffset? QuitDateUtc { get; init; }

    /// <summary>فهرست میل‌استون‌ها با وضعیت هر یک.</summary>
    public IReadOnlyList<MilestoneInfo> Milestones { get; init; } =
        Array.Empty<MilestoneInfo>();

    /// <summary>نمای محاسبه‌شدهٔ بهبود بدن.</summary>
    public Recovery.RecoverySnapshot Recovery { get; init; } = new();
}
