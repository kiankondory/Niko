// ============================================================================
// Niko.Core — NotificationSchedulePolicy.cs
// ----------------------------------------------------------------------------
// مسئولیت: سیاست برنامه‌ریزی اعلان محلی. محاسبهٔ زمان بعدی اعلان (بر پایهٔ زمان
//           روز تنظیم‌شده و «اکنون») و انتخاب کلیدهای محتوای محلی‌سازی برای هر
//           دسته. کاملاً خالص و بدون I/O است.
// وابستگی‌ها و لایه: بخش Domain/Notifications در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: محتوا کوتاه، حمایتی و بدون دادهٔ حساس است. پیش‌فرض امن
//           «غیرفعال» است؛ برنامه‌ریزی فقط برای دسته‌های فعال انجام می‌شود.
// ============================================================================

namespace Niko.Core.Domain.Notifications;

/// <summary>
/// سیاست برنامه‌ریزی و محتوای اعلان‌های محلی.
/// </summary>
public static class NotificationSchedulePolicy
{
    /// <summary>شناسهٔ پایدار اعلان برای یک دسته (برای لغو/برنامه‌ریزی مجدد).</summary>
    public static int GetNotificationId(NotificationCategory category)
        => 1000 + (int)category;

    /// <summary>
    /// محاسبهٔ زمان بعدی وقوع یک زمان روز (UTC) پس از «اکنون».
    /// اگر زمان روز تنظیم نشده باشد null برمی‌گرداند.
    /// </summary>
    public static DateTimeOffset? NextDailyOccurrence(
        DateTimeOffset now,
        TimeOnly? timeOfDay)
    {
        if (timeOfDay is not { } time)
        {
            return null;
        }

        var todayAtTime = new DateTimeOffset(
            now.Year,
            now.Month,
            now.Day,
            time.Hour,
            time.Minute,
            0,
            now.Offset);

        return todayAtTime > now ? todayAtTime : todayAtTime.AddDays(1);
    }

    /// <summary>محتوای محلی‌سازی یک دستهٔ اعلان.</summary>
    public static NotificationContent GetContent(NotificationCategory category)
    {
        return category switch
        {
            NotificationCategory.DailyEncouragement =>
                new NotificationContent(
                    "Notification.Daily.Title",
                    "Notification.Daily.Body"),
            NotificationCategory.MilestoneReached =>
                new NotificationContent(
                    "Notification.Milestone.Title",
                    "Notification.Milestone.Body"),
            NotificationCategory.CravingSupport =>
                new NotificationContent(
                    "Notification.Craving.Title",
                    "Notification.Craving.Body"),
            _ =>
                new NotificationContent(
                    "Notification.Savings.Title",
                    "Notification.Savings.Body"),
        };
    }
}
