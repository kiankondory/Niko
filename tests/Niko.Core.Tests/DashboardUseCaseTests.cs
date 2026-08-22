// ============================================================================
// Niko.Core.Tests — DashboardUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های مورد کاربرد داشبورد: بارگذاری رویدادها و پروفایل از ذخیره‌ساز
//           محلی و تولید نمای پیشرفت. تأیید رفتار آفلاین (بدون شبکه).
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و از FakeClock استفاده می‌کنند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain;
using Niko.Core.Events;
using Niko.Core.UseCases.Dashboard;

namespace Niko.Core.Tests;

public class DashboardUseCaseTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryStore _store;
    private readonly InMemoryUserSettingsStore _settings;
    private readonly DashboardUseCase _useCase;

    public DashboardUseCaseTests()
    {
        _clock = new FakeClock { UtcNow = new DateTimeOffset(2024, 1, 10, 12, 0, 0, TimeSpan.Zero) };
        _store = new InMemoryStore();
        _settings = new InMemoryUserSettingsStore();
        _useCase = new DashboardUseCase(_store, _settings, _clock);
    }

    private async Task AddAsync(EventType type, int day)
    {
        await _store.SaveEventAsync(new LogEvent(
            Guid.NewGuid().ToString("N"),
            new DateTimeOffset(2024, 1, day, 12, 0, 0, TimeSpan.Zero),
            EventSource.Mobile,
            type,
            SyncStatus.Pending));
    }

    [Fact]
    public async Task Execute_LoadsEventsAndReturnsSnapshot()
    {
        await AddAsync(EventType.Smoked, 1);
        await AddAsync(EventType.Smoked, 2);
        await AddAsync(EventType.Resisted, 3);
        await AddAsync(EventType.Craving, 4);

        var result = await _useCase.ExecuteAsync();

        Assert.Equal(4, result.EventCount);
        Assert.Equal(2, result.Snapshot.TotalSmoked);
        Assert.Equal(1, result.Snapshot.TotalResisted);
        Assert.Equal(1, result.Snapshot.TotalCravings);
    }

    [Fact]
    public async Task Execute_ProvidesCurrentDayAggregateWithoutReadingFutureEvents()
    {
        await AddAsync(EventType.Smoked, 10);
        await AddAsync(EventType.Resisted, 10);
        await AddAsync(EventType.Smoked, 9);
        await _store.SaveEventAsync(new LogEvent(
            Guid.NewGuid().ToString("N"),
            new DateTimeOffset(2024, 1, 11, 12, 0, 0, TimeSpan.Zero),
            EventSource.Mobile,
            EventType.Smoked,
            SyncStatus.Pending));

        var result = await _useCase.ExecuteAsync();

        Assert.Equal(1, result.DailySummary.SmokedToday);
        Assert.Equal(1, result.DailySummary.ResistedToday);
    }

    [Fact]
    public async Task Execute_WithProfile_IncludesSavings()
    {
        _settings.Profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0.5m,
        };

        var result = await _useCase.ExecuteAsync();

        // ۵ روز × ۱۰ نخ × ۰٫۵ = ۲۵ (هیچ مصرف واقعی‌ای نیست)
        Assert.Equal(25m, result.Snapshot.ApproximateSavings);
    }

    [Fact]
    public async Task Execute_WithoutProfile_ReturnsNullSavings()
    {
        var result = await _useCase.ExecuteAsync();

        Assert.Null(result.Snapshot.ApproximateSavings);
    }

    [Fact]
    public async Task Execute_WithManyEvents_LoadsAllPages()
    {
        // بیش از یک صفحه (BatchSize=500 در use case) برای اطمینان از بارگذاری کامل.
        for (var i = 0; i < 1200; i++)
        {
            await AddAsync(EventType.Resisted, 1);
        }

        var result = await _useCase.ExecuteAsync();

        Assert.Equal(1200, result.EventCount);
    }
}
