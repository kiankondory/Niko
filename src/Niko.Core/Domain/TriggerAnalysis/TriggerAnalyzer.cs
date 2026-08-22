// ============================================================================
// Niko.Core — TriggerAnalyzer.cs
// ----------------------------------------------------------------------------
// مسئولیت: تحلیل خالص و قطعی الگوهای محرک از رویدادهای محلی. فقط دادهٔ تجمیعی
//           برمی‌گرداند؛ جزئیات خام رویداد هرگز در خروجی نیست. بدون ML و بدون ارسال بیرونی.
// وابستگی‌ها و لایه: بخش Domain/TriggerAnalysis در Core؛ فقط LogEvent را مصرف می‌کند.
// نکات تغییر و قیود: قطعی است (ورودی یکسان → خروجی یکسان، ترتیب پایدار). اگر داده
//           ناکافی باشد (کمتر از آستانه)، بینشی برنمی‌گردد. خروجی تقریبی و غیرپزشکی است.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.TriggerAnalysis;

/// <summary>
/// تحلیل‌گر خالص الگوهای محرک.
/// </summary>
public static class TriggerAnalyzer
{
    private static readonly EventType[] _analyzedTypes =
    {
        EventType.Smoked,
        EventType.Resisted,
        EventType.Craving,
    };

    /// <summary>
    /// تحلیل رویدادها و بازگرداندن بینش‌های تلفیقی.
    /// </summary>
    public static TriggerAnalysisResult Analyze(IReadOnlyList<LogEvent> events)
    {
        var relevant = events
            .Where(e => _analyzedTypes.Contains(e.Type))
            .ToList();

        if (relevant.Count < TriggerAnalysisResult.MinimumDataThreshold)
        {
            return new TriggerAnalysisResult
            {
                IsEnabled = true,
                HasSufficientData = false,
                TotalEventsAnalyzed = relevant.Count,
            };
        }

        var insights = new TriggerInsight?[]
        {
            TimeOfDayInsight(relevant),
            DayOfWeekInsight(relevant),
            ContextInsight(relevant),
            CravingFrequencyInsight(relevant),
            SmokedVsResistedInsight(relevant),
        }
        .OfType<TriggerInsight>()
        .OrderBy(i => i.Kind) // ترتیب پایدار
        .ToList();

        return new TriggerAnalysisResult
        {
            IsEnabled = true,
            HasSufficientData = true,
            TotalEventsAnalyzed = relevant.Count,
            Insights = insights,
        };
    }

    private static TriggerInsight? TimeOfDayInsight(List<LogEvent> events)
    {
        var buckets = events
            .GroupBy(e => BucketForHour(e.OccurredAtUtc.LocalDateTime.Hour))
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .First();

        var strength = Math.Round(buckets.Count() / (double)events.Count * 100.0, 0);
        return new TriggerInsight(
            TriggerInsightKind.TimeOfDay,
            "Trigger.TimeOfDay",
            strength,
            buckets.Count(),
            IsApproximate: true)
        {
            Args = new Dictionary<string, object> { ["bucket"] = BucketKey(buckets.Key) },
        };
    }

    private static TriggerInsight? DayOfWeekInsight(List<LogEvent> events)
    {
        var day = events
            .GroupBy(e => e.OccurredAtUtc.LocalDateTime.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .First();

        var strength = Math.Round(day.Count() / (double)events.Count * 100.0, 0);
        return new TriggerInsight(
            TriggerInsightKind.DayOfWeek,
            "Trigger.DayOfWeek",
            strength,
            day.Count(),
            IsApproximate: true)
        {
            Args = new Dictionary<string, object> { ["day"] = day.Key },
        };
    }

    private static TriggerInsight? ContextInsight(List<LogEvent> events)
    {
        var withContext = events
            .Where(e => e.Metadata.TryGetValue("context", out var c) && !string.IsNullOrWhiteSpace(c))
            .ToList();

        if (withContext.Count == 0)
        {
            return null;
        }

        var top = withContext
            .GroupBy(e => e.Metadata["context"])
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key, StringComparer.Ordinal)
            .First();

        var strength = Math.Round(top.Count() / (double)withContext.Count * 100.0, 0);
        return new TriggerInsight(
            TriggerInsightKind.Context,
            "Trigger.Context",
            strength,
            top.Count(),
            IsApproximate: true)
        {
            Args = new Dictionary<string, object> { ["context"] = top.Key },
        };
    }

    private static TriggerInsight? CravingFrequencyInsight(List<LogEvent> events)
    {
        var cravings = events.Count(e => e.Type == EventType.Craving);
        var days = events
            .Select(e => DateOnly.FromDateTime(e.OccurredAtUtc.LocalDateTime))
            .Distinct()
            .Count();

        var perDay = days == 0 ? 0 : Math.Round(cravings / (double)days, 1);
        return new TriggerInsight(
            TriggerInsightKind.CravingFrequency,
            "Trigger.CravingFrequency",
            Math.Min(100, perDay * 25),
            cravings,
            IsApproximate: true)
        {
            Args = new Dictionary<string, object> { ["count"] = perDay },
        };
    }

    private static TriggerInsight? SmokedVsResistedInsight(List<LogEvent> events)
    {
        var smoked = events.Count(e => e.Type == EventType.Smoked);
        var resisted = events.Count(e => e.Type == EventType.Resisted);
        var total = smoked + resisted;

        if (total == 0)
        {
            return null;
        }

        var resistRatio = Math.Round(resisted / (double)total * 100.0, 0);
        return new TriggerInsight(
            TriggerInsightKind.SmokedVsResisted,
            "Trigger.SmokedVsResisted",
            resistRatio,
            total,
            IsApproximate: true)
        {
            Args = new Dictionary<string, object>
            {
                ["smoked"] = smoked,
                ["resisted"] = resisted,
            },
        };
    }

    private static TimeBucket BucketForHour(int hour)
    {
        if (hour is >= 5 and < 8)
        {
            return TimeBucket.EarlyMorning;
        }

        if (hour is >= 8 and < 12)
        {
            return TimeBucket.Morning;
        }

        if (hour is >= 12 and < 17)
        {
            return TimeBucket.Afternoon;
        }

        if (hour is >= 17 and < 21)
        {
            return TimeBucket.Evening;
        }

        return TimeBucket.Night;
    }

    private static string BucketKey(TimeBucket bucket)
        => $"Trigger.TimeBucket.{bucket}";
}
