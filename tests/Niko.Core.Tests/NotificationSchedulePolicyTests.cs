// ============================================================================
// Niko.Core.Tests — NotificationSchedulePolicyTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های سیاست برنامه‌ریزی اعلان: زمان بعدی روزانه، شناسهٔ پایدار و
//           کلیدهای محتوای محلی‌سازی.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و NotificationSchedulePolicy را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و زمان «اکنون» را صریح می‌گیرند.
// ============================================================================

using Niko.Core.Domain.Notifications;

namespace Niko.Core.Tests;

public class NotificationSchedulePolicyTests
{
    [Fact]
    public void NextDailyOccurrence_NoTime_ReturnsNull()
    {
        var now = new DateTimeOffset(2024, 3, 1, 12, 0, 0, TimeSpan.Zero);
        Assert.Null(NotificationSchedulePolicy.NextDailyOccurrence(now, null));
    }

    [Fact]
    public void NextDailyOccurrence_LaterToday_ReturnsTodayAtTime()
    {
        var now = new DateTimeOffset(2024, 3, 1, 8, 0, 0, TimeSpan.Zero);
        var time = new TimeOnly(18, 30);

        var next = NotificationSchedulePolicy.NextDailyOccurrence(now, time);

        Assert.Equal(new DateTimeOffset(2024, 3, 1, 18, 30, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void NextDailyOccurrence_TimeAlreadyPassed_ReturnsTomorrow()
    {
        var now = new DateTimeOffset(2024, 3, 1, 20, 0, 0, TimeSpan.Zero);
        var time = new TimeOnly(8, 0);

        var next = NotificationSchedulePolicy.NextDailyOccurrence(now, time);

        Assert.Equal(new DateTimeOffset(2024, 3, 2, 8, 0, 0, TimeSpan.Zero), next);
    }

    [Fact]
    public void GetNotificationId_IsStablePerCategory()
    {
        Assert.Equal(1000 + (int)NotificationCategory.DailyEncouragement,
            NotificationSchedulePolicy.GetNotificationId(NotificationCategory.DailyEncouragement));
        Assert.Equal(1000 + (int)NotificationCategory.MilestoneReached,
            NotificationSchedulePolicy.GetNotificationId(NotificationCategory.MilestoneReached));
        Assert.Equal(1000 + (int)NotificationCategory.CravingSupport,
            NotificationSchedulePolicy.GetNotificationId(NotificationCategory.CravingSupport));
        Assert.Equal(1000 + (int)NotificationCategory.SavingsProgress,
            NotificationSchedulePolicy.GetNotificationId(NotificationCategory.SavingsProgress));
    }

    [Fact]
    public void GetContent_ReturnsLocalizedKeys_ForAllCategories()
    {
        foreach (NotificationCategory category in Enum.GetValues<NotificationCategory>())
        {
            var content = NotificationSchedulePolicy.GetContent(category);
            Assert.False(string.IsNullOrWhiteSpace(content.TitleKey));
            Assert.False(string.IsNullOrWhiteSpace(content.BodyKey));
            Assert.StartsWith("Notification.", content.TitleKey);
            Assert.StartsWith("Notification.", content.BodyKey);
        }
    }
}
