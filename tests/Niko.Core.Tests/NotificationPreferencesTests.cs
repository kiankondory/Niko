// ============================================================================
// Niko.Core.Tests — NotificationPreferencesTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های ترجیحات اعلان: پیش‌فرض امن (همه غیرفعال)، فعال/غیرفعال هر
//           دسته و ویژگی IsAnythingEnabled.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و NotificationPreferences را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند.
// ============================================================================

using Niko.Core.Domain.Notifications;

namespace Niko.Core.Tests;

public class NotificationPreferencesTests
{
    [Fact]
    public void Default_AllCategoriesDisabled_IsSafe()
    {
        var prefs = new NotificationPreferences();

        Assert.False(prefs.DailyEncouragementEnabled);
        Assert.False(prefs.MilestoneReachedEnabled);
        Assert.False(prefs.CravingSupportEnabled);
        Assert.False(prefs.SavingsProgressEnabled);
        Assert.False(prefs.IsAnythingEnabled);
    }

    [Fact]
    public void WithEnabled_EnablesSingleCategory()
    {
        var prefs = new NotificationPreferences()
            .WithEnabled(NotificationCategory.DailyEncouragement, true);

        Assert.True(prefs.DailyEncouragementEnabled);
        Assert.False(prefs.MilestoneReachedEnabled);
        Assert.True(prefs.IsAnythingEnabled);
    }

    [Fact]
    public void WithEnabled_DisablesCategory()
    {
        var prefs = new NotificationPreferences()
            .WithEnabled(NotificationCategory.CravingSupport, true)
            .WithEnabled(NotificationCategory.CravingSupport, false);

        Assert.False(prefs.CravingSupportEnabled);
        Assert.False(prefs.IsAnythingEnabled);
    }

    [Fact]
    public void IsEnabled_ReflectsCategoryState()
    {
        var prefs = new NotificationPreferences()
            .WithEnabled(NotificationCategory.SavingsProgress, true);

        Assert.True(prefs.IsEnabled(NotificationCategory.SavingsProgress));
        Assert.False(prefs.IsEnabled(NotificationCategory.DailyEncouragement));
    }
}
