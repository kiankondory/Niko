// ============================================================================
// Niko.Core.Tests — CompanionDailySummaryTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون قطعی شمارش روزانهٔ ابزارک از رویدادهای ذخیره‌شده.
// وابستگی‌ها و لایه: تست Core → CompanionDailySummaryCalculator؛ بدون Android،
//           SQLite، شبکه یا دادهٔ شخصی واقعی.
// نکات تغییر و قیود: مرز روز، آینده، نوع نامعتبر، حذف منطقی و عدم تکرار شناسه
//           باید privacy-safe و مستقل از منطقهٔ زمانی ماشین باشند.
// ============================================================================

using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public sealed class CompanionDailySummaryTests
{
    private static readonly TimeZoneInfo PlusTwo =
        TimeZoneInfo.CreateCustomTimeZone("Niko+02", TimeSpan.FromHours(2), "Niko +02", "Niko +02");

    private static readonly DateTimeOffset NowUtc =
        new(2026, 8, 21, 21, 30, 0, TimeSpan.Zero);

    [Fact]
    public void EmptyDay_ReturnsZeroCounts()
    {
        var result = CompanionDailySummaryCalculator.Calculate(Array.Empty<LogEvent>(), NowUtc, PlusTwo);

        Assert.Equal(0, result.SmokedToday);
        Assert.Equal(0, result.ResistedToday);
    }

    [Fact]
    public void CountsOnlyCurrentLocalDayAndValidPastEvents()
    {
        var events = new[]
        {
            Event("today-smoked", EventType.Smoked, new DateTimeOffset(2026, 8, 21, 0, 30, 0, TimeSpan.Zero)),
            Event("today-resisted", EventType.Resisted, new DateTimeOffset(2026, 8, 21, 20, 0, 0, TimeSpan.Zero)),
            Event("yesterday", EventType.Smoked, new DateTimeOffset(2026, 8, 20, 21, 59, 0, TimeSpan.Zero)),
            Event("future", EventType.Smoked, new DateTimeOffset(2026, 8, 21, 22, 0, 0, TimeSpan.Zero)),
            Event("deleted", EventType.Deleted, new DateTimeOffset(2026, 8, 21, 19, 0, 0, TimeSpan.Zero)),
            Event("invalid", (EventType)999, new DateTimeOffset(2026, 8, 21, 19, 0, 0, TimeSpan.Zero)),
        };

        var result = CompanionDailySummaryCalculator.Calculate(events, NowUtc, PlusTwo);

        Assert.Equal(1, result.SmokedToday);
        Assert.Equal(1, result.ResistedToday);
    }

    [Fact]
    public void LocalDayBoundary_UsesActiveTimezone()
    {
        var events = new[]
        {
            Event("before-local-midnight", EventType.Smoked, new DateTimeOffset(2026, 8, 20, 21, 59, 59, TimeSpan.Zero)),
            Event("at-local-midnight", EventType.Smoked, new DateTimeOffset(2026, 8, 20, 22, 0, 0, TimeSpan.Zero)),
        };

        var result = CompanionDailySummaryCalculator.Calculate(events, NowUtc, PlusTwo);

        Assert.Equal(1, result.SmokedToday);
    }

    private static LogEvent Event(string id, EventType type, DateTimeOffset occurredAtUtc)
        => new(id, occurredAtUtc, EventSource.Widget, type, SyncStatus.Pending);
}
