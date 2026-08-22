// ============================================================================
// Niko.Core — NotificationPreferences.cs
// ----------------------------------------------------------------------------
// مسئولیت: ترجیحات اعلان کاربر. به‌صورت پیش‌فرض همهٔ دسته‌ها غیرفعال‌اند (امن).
//           هر دسته می‌تواند فعال و زمان روز آن تنظیم شود.
// وابستگی‌ها و لایه: بخش Domain/Notifications در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: محتوای اعلان‌ها حساس نیستند؛ پیش‌نمایش حاوی دادهٔ مصرف/سلامت
//           نیست. پیش‌فرض امن «غیرفعال» است.
// ============================================================================

namespace Niko.Core.Domain.Notifications;

/// <summary>
/// ترجیحات اعلان کاربر.
/// </summary>
public sealed record NotificationPreferences
{
    /// <summary>فعال بودن دستهٔ تشویق روزانه.</summary>
    public bool DailyEncouragementEnabled { get; init; }

    /// <summary>فعال بودن دستهٔ میل‌استون.</summary>
    public bool MilestoneReachedEnabled { get; init; }

    /// <summary>فعال بودن دستهٔ پشتیبانی هوس.</summary>
    public bool CravingSupportEnabled { get; init; }

    /// <summary>فعال بودن دستهٔ پیشرفت/صرفه‌جویی.</summary>
    public bool SavingsProgressEnabled { get; init; }

    /// <summary>زمان روز برای اعلان‌های روزانه (در صورت تنظیم).</summary>
    public TimeOnly? TimeOfDay { get; init; }

    /// <summary>آیا حداقل یک دسته فعال است؟</summary>
    public bool IsAnythingEnabled =>
        DailyEncouragementEnabled ||
        MilestoneReachedEnabled ||
        CravingSupportEnabled ||
        SavingsProgressEnabled;

    /// <summary>آیا یک دستهٔ مشخص فعال است؟</summary>
    public bool IsEnabled(NotificationCategory category)
    {
        return category switch
        {
            NotificationCategory.DailyEncouragement => DailyEncouragementEnabled,
            NotificationCategory.MilestoneReached => MilestoneReachedEnabled,
            NotificationCategory.CravingSupport => CravingSupportEnabled,
            NotificationCategory.SavingsProgress => SavingsProgressEnabled,
            _ => false,
        };
    }

    /// <summary>بازگشت یک نسخه با وضعیت فعال/غیرفعال یک دسته.</summary>
    public NotificationPreferences WithEnabled(NotificationCategory category, bool enabled)
    {
        return category switch
        {
            NotificationCategory.DailyEncouragement => this with { DailyEncouragementEnabled = enabled },
            NotificationCategory.MilestoneReached => this with { MilestoneReachedEnabled = enabled },
            NotificationCategory.CravingSupport => this with { CravingSupportEnabled = enabled },
            NotificationCategory.SavingsProgress => this with { SavingsProgressEnabled = enabled },
            _ => this,
        };
    }
}
