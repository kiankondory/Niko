// ============================================================================
// Niko.Core — IslandDailyReportCalculator.cs
// ----------------------------------------------------------------------------
// مسئولیت: محاسبهٔ قطعی گزارش روزانهٔ جزیره و پس‌انداز تجمعی از تاریخ ترک تا
//           روز محلی فعلی با استفاده از رویدادهای معتبر و قیمت مؤثر هر نخ.
// وابستگی‌ها و لایه: Domain در Core؛ فقط از LogEvent و UserProfile استفاده می‌کند.
// نکات تغییر و قیود: مرز روز با TimeZoneInfo تعیین می‌شود؛ رویداد آینده، تکراری
//           و نوع Deleted نادیده گرفته می‌شود و هیچ منطق ذخیره‌سازی ندارد.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.Island;

/// <summary>محاسبه‌گر خالص گزارش مصرف و پس‌انداز روزانهٔ جزیره.</summary>
public static class IslandDailyReportCalculator
{
    public static IReadOnlyList<IslandDailyReport> Calculate(
        IEnumerable<LogEvent> events,
        UserProfile? profile,
        DateTimeOffset nowUtc,
        TimeZoneInfo localTimeZone)
    {
        if (profile?.QuitDateUtc is not { } quitDate)
        {
            return Array.Empty<IslandDailyReport>();
        }

        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(nowUtc, localTimeZone).Date);
        var start = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(quitDate, localTimeZone).Date);
        if (start > today)
        {
            return Array.Empty<IslandDailyReport>();
        }

        var price = profile.EffectivePricePerCigarette;
        var daily = events
            .Where(e => e.Type is EventType.Smoked or EventType.Resisted)
            .Where(e => e.OccurredAtUtc <= nowUtc)
            .GroupBy(e => e.EventId, StringComparer.Ordinal)
            .Select(group => group.First())
            .Select(e => (Event: e, Date: DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(e.OccurredAtUtc, localTimeZone).Date)))
            .Where(item => item.Date >= start && item.Date <= today)
            .GroupBy(item => item.Date)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Smoked = group.Count(item => item.Event.Type == EventType.Smoked),
                    Resisted = group.Count(item => item.Event.Type == EventType.Resisted),
                });

        var result = new List<IslandDailyReport>(today.DayNumber - start.DayNumber + 1);
        for (var date = start; date <= today; date = date.AddDays(1))
        {
            daily.TryGetValue(date, out var counts);
            result.Add(new IslandDailyReport(
                date,
                counts?.Smoked ?? 0,
                counts?.Resisted ?? 0,
                price is { } value ? (counts?.Resisted ?? 0) * value : null));
        }

        return result;
    }

    public static decimal? CalculateCumulativeSavings(IReadOnlyList<IslandDailyReport> reports)
    {
        if (reports.Any(report => report.SavedAmount is null))
        {
            return null;
        }

        return reports.Sum(report => report.SavedAmount ?? 0m);
    }
}

