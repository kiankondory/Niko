// ============================================================================
// Niko.Core — StreakCalculator.cs
// ----------------------------------------------------------------------------
// مسئولیت: محاسبهٔ خالص طول استریک بر حسب روز بر پایهٔ رویدادهای مقاومت و مصرف.
//           هیچ وابستگی به I/O یا پلتفرم ندارد و کاملاً قابل تست است.
// وابستگی‌ها و لایه: بخش Domain در Core؛ فقط LogEvent را مصرف می‌کند.
// نکات تغییر و قیود: استریک بر پایهٔ تاریخ UTC محاسبه می‌شود. یک روز با «مصرف»
//           استریک را می‌شکند؛ یک روز بدون «مقاومت» نیز تداوم را قطع می‌کند.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain;

/// <summary>
/// محاسبه‌گر خالص استریک مقاومت.
/// </summary>
public static class StreakCalculator
{
    /// <summary>
    /// محاسبهٔ طول استریک (روزهای متوالی مقاومت بدون مصرف) تا آخرین روز مقاومت.
    /// </summary>
    public static int Calculate(IEnumerable<LogEvent> events)
    {
        var resistedDates = new HashSet<DateOnly>(
            events
                .Where(e => e.Type == EventType.Resisted)
                .Select(e => DateOnly.FromDateTime(e.OccurredAtUtc.UtcDateTime)));

        var smokedDates = new HashSet<DateOnly>(
            events
                .Where(e => e.Type == EventType.Smoked)
                .Select(e => DateOnly.FromDateTime(e.OccurredAtUtc.UtcDateTime)));

        if (resistedDates.Count == 0)
        {
            return 0;
        }

        var current = resistedDates.Max();
        var streak = 0;

        while (resistedDates.Contains(current) && !smokedDates.Contains(current))
        {
            streak++;
            current = current.AddDays(-1);
        }

        return streak;
    }
}
