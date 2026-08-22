// ============================================================================
// Niko.Core.Tests — NotificationSettingsUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های مورد کاربرد تنظیمات اعلان: درخواست مجوز فقط هنگام فعال‌سازی
//           (opt-in)، برنامه‌ریزی دسته‌های فعال، لغو هنگام غیرفعال‌سازی و رفتار رد مجوز.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و از FakeClock استفاده می‌کنند.
// ============================================================================

using Niko.Core.Domain.Notifications;
using Niko.Core.UseCases.Notifications;

namespace Niko.Core.Tests;

public class NotificationSettingsUseCaseTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryNotificationPreferencesStore _store;
    private readonly FakeNotificationService _service;
    private readonly NotificationSettingsUseCase _useCase;

    public NotificationSettingsUseCaseTests()
    {
        _clock = new FakeClock { UtcNow = new DateTimeOffset(2024, 3, 1, 8, 0, 0, TimeSpan.Zero) };
        _store = new InMemoryNotificationPreferencesStore();
        _service = new FakeNotificationService();
        _useCase = new NotificationSettingsUseCase(_store, _service, _clock);
    }

    [Fact]
    public async Task Load_WithNoSavedPreferences_ReturnsSafeDefaults()
    {
        var prefs = await _useCase.LoadAsync();

        Assert.False(prefs.IsAnythingEnabled);
    }

    [Fact]
    public async Task Save_AllDisabled_DoesNotRequestPermission()
    {
        var result = await _useCase.SaveAsync(new NotificationPreferences());

        Assert.Equal(0, _service.PermissionRequests);
        Assert.True(result.PermissionGranted);
        Assert.False(result.PermissionDenied);
        Assert.True(_service.CancelAllCalls >= 1);
    }

    [Fact]
    public async Task Save_EnableCategory_RequestsPermissionAndSchedules()
    {
        var prefs = new NotificationPreferences()
            .WithEnabled(NotificationCategory.DailyEncouragement, true)
            with { TimeOfDay = new TimeOnly(18, 0) };

        var result = await _useCase.SaveAsync(prefs);

        Assert.Equal(1, _service.PermissionRequests); // opt-in فقط یک بار
        Assert.True(result.PermissionGranted);
        Assert.False(result.PermissionDenied);
        // یک اعلان روزانه برای دستهٔ فعال برنامه‌ریزی می‌شود.
        var scheduled = Assert.Single(_service.Scheduled);
        Assert.Equal(
            NotificationSchedulePolicy.GetNotificationId(NotificationCategory.DailyEncouragement),
            scheduled.Id);
        Assert.Equal(new DateTimeOffset(2024, 3, 1, 18, 0, 0, TimeSpan.Zero), scheduled.FireAt);
    }

    [Fact]
    public async Task Save_EnableAllCategories_SchedulesFour()
    {
        var prefs = new NotificationPreferences
        {
            DailyEncouragementEnabled = true,
            MilestoneReachedEnabled = true,
            CravingSupportEnabled = true,
            SavingsProgressEnabled = true,
            TimeOfDay = new TimeOnly(9, 0),
        };

        var result = await _useCase.SaveAsync(prefs);

        Assert.Equal(4, _service.Scheduled.Count);
        Assert.True(result.PermissionGranted);
    }

    [Fact]
    public async Task Save_PermissionDenied_SchedulesNothingAndReportsDenied()
    {
        _service.GrantPermission = false;
        var prefs = new NotificationPreferences()
            .WithEnabled(NotificationCategory.CravingSupport, true)
            with { TimeOfDay = new TimeOnly(10, 0) };

        var result = await _useCase.SaveAsync(prefs);

        Assert.True(result.PermissionDenied);
        Assert.False(result.PermissionGranted);
        Assert.Empty(_service.Scheduled);
        // ترجیحات همچنان ذخیره می‌شوند.
        Assert.NotNull(_store.Preferences);
    }

    [Fact]
    public async Task Save_DisableAll_CancelsSchedules()
    {
        await _useCase.SaveAsync(new NotificationPreferences
        {
            DailyEncouragementEnabled = true,
            TimeOfDay = new TimeOnly(9, 0),
        });

        var cancelCallsBefore = _service.CancelAllCalls;

        var result = await _useCase.SaveAsync(new NotificationPreferences());

        Assert.True(_service.CancelAllCalls > cancelCallsBefore);
        Assert.True(result.PermissionGranted);
    }
}
