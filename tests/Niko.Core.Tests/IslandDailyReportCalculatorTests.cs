// ============================================================================
// Niko.Core.Tests — IslandDailyReportCalculatorTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون قطعی شمارش روزانهٔ جزیره و پس‌انداز تجمعی.
// وابستگی‌ها و لایه: لایهٔ تست Core؛ فقط مدل‌های دامنه و زمان/منطقهٔ زمانی ساختگی.
// نکات تغییر و قیود: رویدادهای آینده، تکراری و خارج از بازه نباید در گزارش دیده
//           شوند و تست‌ها نباید به دستگاه یا پایگاه‌دادهٔ واقعی وابسته باشند.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.Island;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public sealed class IslandDailyReportCalculatorTests
{
    private static readonly TimeZoneInfo PlusTwo = TimeZoneInfo.CreateCustomTimeZone(
        "Test/PlusTwo", TimeSpan.FromHours(2), "Test", "Test");

    [Fact]
    public void Calculate_GroupsSmokedAndResistedByLocalDayAndSumsSavings()
    {
        var reports = IslandDailyReportCalculator.Calculate(
            new[]
            {
                Event(EventType.Smoked, 1, 22), // Jan 2 local
                Event(EventType.Resisted, 1, 23), // Jan 2 local
                Event(EventType.Resisted, 2, 23), // Jan 3 local
            },
            Profile(),
            Utc(3, 12),
            PlusTwo);

        Assert.Equal(3, reports.Count);
        Assert.Equal(1, reports[1].SmokedCount);
        Assert.Equal(1, reports[1].ResistedCount);
        Assert.Equal(1.5m, reports[1].SavedAmount);
        Assert.Equal(1.5m, reports[2].SavedAmount);
        Assert.Equal(3m, IslandDailyReportCalculator.CalculateCumulativeSavings(reports));
    }

    [Fact]
    public void Calculate_ExcludesFutureOutsideRangeDeletedAndDuplicateEvents()
    {
        var duplicateId = Guid.NewGuid().ToString("N");
        var events = new[]
        {
            new LogEvent(duplicateId, Utc(2, 10), EventSource.Mobile, EventType.Smoked, SyncStatus.Pending),
            new LogEvent(duplicateId, Utc(2, 11), EventSource.Mobile, EventType.Smoked, SyncStatus.Pending),
            Event(EventType.Smoked, 3, 13), // future relative to now
            Event(EventType.Deleted, 2, 12),
            Event(EventType.Smoked, 31, 12), // before quit date (previous month)
        };

        var reports = IslandDailyReportCalculator.Calculate(events, Profile(), Utc(3, 12), PlusTwo);

        Assert.Equal(1, reports[1].SmokedCount);
        Assert.Equal(0, reports[2].SmokedCount);
    }

    [Fact]
    public void Calculate_WithoutQuitDateOrPriceReturnsSafeUnavailableValues()
    {
        var reports = IslandDailyReportCalculator.Calculate(
            new[] { Event(EventType.Resisted, 2, 10) },
            new UserProfile { PricePerCigarette = 1m },
            Utc(3, 12),
            PlusTwo);

        Assert.Empty(reports);
        Assert.Equal(0m, IslandDailyReportCalculator.CalculateCumulativeSavings(reports));
    }

    private static UserProfile Profile() => new()
    {
        QuitDateUtc = Utc(1, 0),
        PricePerCigarette = 1.5m,
        CurrencyCode = "USD",
    };

    private static LogEvent Event(EventType type, int day, int hour) => new(
        Guid.NewGuid().ToString("N"), Utc(day, hour), EventSource.Mobile, type, SyncStatus.Pending);

    private static DateTimeOffset Utc(int day, int hour) =>
        day == 31
            ? new(2023, 12, 31, hour, 0, 0, TimeSpan.Zero)
            : new(2024, 1, day, hour, 0, 0, TimeSpan.Zero);
}
