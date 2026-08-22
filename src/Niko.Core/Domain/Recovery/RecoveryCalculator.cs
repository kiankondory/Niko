// ============================================================================
// Niko.Core — RecoveryCalculator.cs
// ----------------------------------------------------------------------------
// مسئولیت: محاسبهٔ خالص مرحلهٔ بهبود بر پایهٔ زمان بدون مصرف. زمان بدون مصرف از
//           تاریخ ترک (اگر موجود باشد) یا آخرین رویداد مقاومت استنتاج می‌شود.
//           کاملاً بدون I/O و قابل تست.
// وابستگی‌ها و لایه: بخش Domain/Recovery در Core؛ از LogEvent و UserProfile استفاده می‌کند.
// نکات تغییر و قیود: نتایج تقریبی و غیرپزشکی‌اند. بدون دادهٔ کافی، مرحلهٔ صفر
//           و بدون زمان بدون مصرف برمی‌گردد.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.Recovery;

/// <summary>
/// محاسبه‌گر خالص بهبود بدن.
/// </summary>
public static class RecoveryCalculator
{
    /// <summary>فهرست مراحل با بازهٔ روز (ToDays برای آخرین مرحله null است).</summary>
    public static readonly IReadOnlyList<RecoveryStageInfo> Stages =
    [
        new(RecoveryStage.Stage0, 0, 1, "Recovery.Stage0"),
        new(RecoveryStage.Stage1, 1, 3, "Recovery.Stage1"),
        new(RecoveryStage.Stage2, 3, 7, "Recovery.Stage2"),
        new(RecoveryStage.Stage3, 7, 14, "Recovery.Stage3"),
        new(RecoveryStage.Stage4, 14, 30, "Recovery.Stage4"),
        new(RecoveryStage.Stage5, 30, 90, "Recovery.Stage5"),
        new(RecoveryStage.Stage6, 90, 180, "Recovery.Stage6"),
        new(RecoveryStage.Stage7, 180, null, "Recovery.Stage7"),
    ];

    /// <summary>
    /// محاسبهٔ نمای بهبود از رویدادها، پروفایل و زمان «اکنون».
    /// </summary>
    public static RecoverySnapshot Calculate(
        IEnumerable<LogEvent> events,
        UserProfile? profile,
        DateTimeOffset now)
    {
        var smokeFree = TryGetSmokeFreeTime(events, profile, now, out var hasData);

        if (!hasData)
        {
            return new RecoverySnapshot
            {
                Stage = RecoveryStage.Stage0,
                ProgressPercent = 0,
                SmokeFreeTime = TimeSpan.Zero,
                HasSufficientData = false,
            };
        }

        var stage = GetStage(smokeFree);
        var percent = GetProgressPercent(smokeFree, stage);

        return new RecoverySnapshot
        {
            Stage = stage.Stage,
            ProgressPercent = percent,
            SmokeFreeTime = smokeFree,
            HasSufficientData = true,
        };
    }

    private static TimeSpan TryGetSmokeFreeTime(
        IEnumerable<LogEvent> events,
        UserProfile? profile,
        DateTimeOffset now,
        out bool hasData)
    {
        hasData = false;

        // اولویت با تاریخ ترک تنظیم‌شده است.
        if (profile?.QuitDateUtc is { } quitDate)
        {
            hasData = true;
            return MaxZero(now - quitDate);
        }

        // در غیر این صورت، آخرین رویداد مقاومت به‌عنوان نقطهٔ مرجع تقریبی.
        var lastResist = events
            .Where(e => e.Type == EventType.Resisted)
            .Select(e => e.OccurredAtUtc)
            .DefaultIfEmpty()
            .Max();

        if (lastResist != default)
        {
            hasData = true;
            return MaxZero(now - lastResist);
        }

        return TimeSpan.Zero;
    }

    private static TimeSpan MaxZero(TimeSpan value) => value < TimeSpan.Zero ? TimeSpan.Zero : value;

    private static RecoveryStageInfo GetStage(TimeSpan smokeFree)
    {
        var days = smokeFree.TotalDays;

        foreach (var stage in Stages)
        {
            if (days < stage.ToDays || stage.ToDays is null)
            {
                return stage;
            }
        }

        return Stages[^1];
    }

    private static double GetProgressPercent(TimeSpan smokeFree, RecoveryStageInfo stage)
    {
        if (stage.ToDays is not { } toDays)
        {
            return 100.0; // آخرین مرحله
        }

        var fromDays = stage.FromDays;
        var days = smokeFree.TotalDays;
        var range = toDays - fromDays;

        if (range <= 0)
        {
            return 0;
        }

        var percent = (days - fromDays) / range * 100.0;
        return Math.Clamp(percent, 0, 100);
    }
}
