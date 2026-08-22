// ============================================================================
// Niko.Infrastructure.Tests — NotificationPreferencesStoreTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های یکپارچهٔ ذخیره/بازیابی ترجیحات اعلان در SQLite، از جمله
//           پیش‌فرض امن و پایداری بین دو نمونهٔ ذخیره‌ساز.
// وابستگی‌ها و لایه: لایهٔ تست؛ Infrastructure و Core را استفاده می‌کند.
// نکات تغییر و قیود: از پایگاه‌دادهٔ موقت هر اجرا استفاده می‌کند؛ بدون شبکه.
// ============================================================================

using Niko.Core.Domain.Notifications;
using Niko.Infrastructure.Persistence;

namespace Niko.Infrastructure.Tests;

public class NotificationPreferencesStoreTests
{
    private static string NewTempPath()
        => Path.Combine(Path.GetTempPath(), $"niko_{Guid.NewGuid():N}.db");

    [Fact]
    public async Task Get_WithNoSavedPreferences_ReturnsNull()
    {
        var store = new NotificationPreferencesStore(NewTempPath());
        var prefs = await store.GetAsync();

        Assert.Null(prefs);
    }

    [Fact]
    public async Task SaveThenGet_RoundTrips()
    {
        var store = new NotificationPreferencesStore(NewTempPath());
        var prefs = new NotificationPreferences
        {
            DailyEncouragementEnabled = true,
            MilestoneReachedEnabled = true,
            CravingSupportEnabled = false,
            SavingsProgressEnabled = true,
            TimeOfDay = new TimeOnly(18, 30),
        };

        await store.SaveAsync(prefs);
        var loaded = await store.GetAsync();

        Assert.NotNull(loaded);
        Assert.True(loaded.DailyEncouragementEnabled);
        Assert.True(loaded.MilestoneReachedEnabled);
        Assert.False(loaded.CravingSupportEnabled);
        Assert.True(loaded.SavingsProgressEnabled);
        Assert.Equal(new TimeOnly(18, 30), loaded.TimeOfDay);
    }

    [Fact]
    public async Task SaveThenUpdate_OverwritesRow()
    {
        var store = new NotificationPreferencesStore(NewTempPath());
        await store.SaveAsync(new NotificationPreferences
        {
            DailyEncouragementEnabled = true,
            TimeOfDay = new TimeOnly(9, 0),
        });
        await store.SaveAsync(new NotificationPreferences
        {
            DailyEncouragementEnabled = false,
            TimeOfDay = new TimeOnly(20, 15),
        });

        var loaded = await store.GetAsync();

        Assert.False(loaded!.DailyEncouragementEnabled);
        Assert.Equal(new TimeOnly(20, 15), loaded.TimeOfDay);
    }

    [Fact]
    public async Task PersistsAcrossNewStoreInstance_SameFile()
    {
        var path = NewTempPath();
        var store1 = new NotificationPreferencesStore(path);
        await store1.SaveAsync(new NotificationPreferences
        {
            CravingSupportEnabled = true,
            TimeOfDay = new TimeOnly(12, 0),
        });

        var store2 = new NotificationPreferencesStore(path);
        var loaded = await store2.GetAsync();

        Assert.True(loaded!.CravingSupportEnabled);
        Assert.Equal(new TimeOnly(12, 0), loaded.TimeOfDay);
    }
}
