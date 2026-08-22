// ============================================================================
// Niko.Core — ProgressCalculator.cs
// ----------------------------------------------------------------------------
// مسئولیت: محاسبهٔ خالص داده‌های داشبورد: شمارش رویدادها، استریک، پیشرفت به سمت
//           میل‌استون و صرفه‌جویی تقریبی. کاملاً بدون I/O و قابل تست.
// وابستگی‌ها و لایه: بخش Domain در Core؛ از StreakCalculator و UserProfile استفاده می‌کند.
// نکات تغییر و قیود: میل‌استون‌ها بر پایهٔ روزهای استریک‌اند. صرفه‌جویی تقریبی
//           است و فقط با ورودی کامل پروفایل محاسبه می‌شود.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain;

/// <summary>
/// محاسبه‌گر خالص داده‌های پیشرفت داشبورد.
/// </summary>
public static class ProgressCalculator
{
    /// <summary>آستانه‌های میل‌استون بر پایهٔ روزهای استریک.</summary>
    public static readonly int[] MilestoneThresholds =
    {
        1, 3, 7, 14, 30, 60, 90, 180, 365,
    };

    /// <summary>
    /// محاسبهٔ نمای پیشرفت از رویدادها و پروفایل.
    /// </summary>
    public static ProgressSnapshot Calculate(
        IEnumerable<LogEvent> events,
        UserProfile? profile,
        DateOnly today)
    {
        var eventList = events.ToList();

        var totalSmoked = eventList.Count(e => e.Type == EventType.Smoked);
        var totalResisted = eventList.Count(e => e.Type == EventType.Resisted);
        var totalCravings = eventList.Count(e => e.Type == EventType.Craving);
        var streakDays = StreakCalculator.Calculate(eventList);

        var (currentMilestone, nextMilestone) = GetMilestones(streakDays);
        var milestonePercent = nextMilestone == currentMilestone
            ? 100.0
            : Math.Round((double)(streakDays - currentMilestone) /
                         (nextMilestone - currentMilestone) * 100.0, 0);

        var savings = profile is { HasSavingsInput: true }
            ? CalculateSavings(eventList, profile, today)
            : null;

        var now = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var recovery = Recovery.RecoveryCalculator.Calculate(eventList, profile, now);

        return new ProgressSnapshot
        {
            TotalSmoked = totalSmoked,
            TotalResisted = totalResisted,
            TotalCravings = totalCravings,
            CurrentStreakDays = streakDays,
            CurrentMilestoneDays = currentMilestone,
            NextMilestoneDays = nextMilestone,
            MilestoneProgressPercent = milestonePercent,
            ApproximateSavings = savings,
            QuitDateUtc = profile?.QuitDateUtc,
            Milestones = MilestoneCalculator.GetMilestones(streakDays),
            Recovery = recovery,
        };
    }

    /// <summary>
    /// صرفه‌جویی تقریبی: (روزهای ترک × مصرف روزانه − نخ‌های واقعی مصرف‌شده) × قیمت مؤثر هر نخ.
    /// در صورت نبودن دادهٔ کافی یا مقادیر نامعتبر null برمی‌گرداند.
    /// </summary>
    public static decimal? CalculateSavings(
        IEnumerable<LogEvent> events,
        UserProfile profile,
        DateOnly today)
    {
        if (!profile.HasSavingsInput ||
            profile.QuitDateUtc is not { } quitDate ||
            profile.CigarettesPerDay is not { } cpd ||
            profile.EffectivePricePerCigarette is not { } price)
        {
            return null;
        }

        var eventList = events.ToList();
        var quitDay = DateOnly.FromDateTime(quitDate.UtcDateTime);
        var daysSinceQuit = Math.Max(0, today.DayNumber - quitDay.DayNumber);

        var actuallySmoked = eventList.Count(e => e.Type == EventType.Smoked);
        var avoidedCigarettes = Math.Max(0, daysSinceQuit * cpd - actuallySmoked);

        return avoidedCigarettes * price;
    }

    private static (int Current, int Next) GetMilestones(int streakDays)
    {
        var current = 0;
        var next = MilestoneThresholds[0];

        foreach (var threshold in MilestoneThresholds)
        {
            if (threshold <= streakDays)
            {
                current = threshold;
            }
            else
            {
                next = threshold;
                break;
            }
        }

        return (current, next);
    }
}
