// ============================================================================
// Niko.Core — CompanionDailySummaryCalculator.cs
// ----------------------------------------------------------------------------
// مسئولیت: محاسبهٔ شمارش‌های تجمیعی روز محلی برای ابزارک و همراه‌ها، بدون نمایش
//           رویداد خام یا نگهداری ذخیره‌سازی موازی.
// وابستگی‌ها و لایه: Domain/CompanionContracts در Core؛ فقط به LogEvent و زمان
//           تزریق‌شده وابسته است و از UI، Android و شبکه مستقل می‌ماند.
// نکات تغییر و قیود: فقط Smoked، Resisted و Craving معتبرِ تا زمان فعلی شمرده می‌شوند؛
//           مرز روز با TimeZoneInfo فعال تعیین می‌شود و رویداد آینده، حذف‌شده
//           یا نوع ناشناخته هرگز در شمارش وارد نمی‌شود.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>خلاصهٔ شمارش‌های امن روز محلی.</summary>
public sealed record CompanionDailySummary(int SmokedToday, int ResistedToday)
{
    /// <summary>رویدادهای معتبر هوس در روز محلی جاری.</summary>
    public int CravingsToday { get; init; }
}

/// <summary>محاسبه‌گر قطعی شمارش‌های روزانهٔ ابزارک.</summary>
public static class CompanionDailySummaryCalculator
{
    public static CompanionDailySummary Calculate(
        IEnumerable<LogEvent> events,
        DateTimeOffset utcNow,
        TimeZoneInfo localTimeZone)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(localTimeZone);

        var localNow = TimeZoneInfo.ConvertTime(utcNow, localTimeZone);
        var localDate = DateOnly.FromDateTime(localNow.DateTime);
        var validToday = events
            .Where(logEvent => logEvent.OccurredAtUtc <= utcNow)
            .Where(logEvent => logEvent.Type is EventType.Smoked or EventType.Resisted or EventType.Craving)
            .Where(logEvent => DateOnly.FromDateTime(
                TimeZoneInfo.ConvertTime(logEvent.OccurredAtUtc, localTimeZone).DateTime) == localDate)
            .ToList();

        return new CompanionDailySummary(
            validToday.Count(logEvent => logEvent.Type == EventType.Smoked),
            validToday.Count(logEvent => logEvent.Type == EventType.Resisted))
        {
            CravingsToday = validToday.Count(logEvent => logEvent.Type == EventType.Craving),
        };
    }
}
