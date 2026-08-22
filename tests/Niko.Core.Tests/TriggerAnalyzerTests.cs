// ============================================================================
// Niko.Core.Tests — TriggerAnalyzerTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون قطعی تحلیل محرک، کفایت داده و خروجی تجمیعی بدون نمایش رویداد خام.
// وابستگی‌ها و لایه: تست Core؛ فقط Domain/TriggerAnalysis و مدل رویداد را مصرف می‌کند.
// نکات تغییر و قیود: زمان‌ها، شناسه‌ها و ابرداده ثابت‌اند؛ هیچ شبکه یا دادهٔ کاربر واقعی استفاده نمی‌شود.
// ============================================================================

using Niko.Core.Domain.TriggerAnalysis;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public sealed class TriggerAnalyzerTests
{
    [Fact]
    public void Analyze_WithEmptyData_ReturnsSafeInsufficientResult()
    {
        var result = TriggerAnalyzer.Analyze(Array.Empty<LogEvent>());

        Assert.True(result.IsEnabled);
        Assert.False(result.HasSufficientData);
        Assert.Equal(0, result.TotalEventsAnalyzed);
        Assert.Empty(result.Insights);
    }

    [Fact]
    public void Analyze_BelowMinimumThreshold_ReturnsNoInsights()
    {
        var events = CreateEvents(TriggerAnalysisResult.MinimumDataThreshold - 1);

        var result = TriggerAnalyzer.Analyze(events);

        Assert.False(result.HasSufficientData);
        Assert.Equal(TriggerAnalysisResult.MinimumDataThreshold - 1, result.TotalEventsAnalyzed);
        Assert.Empty(result.Insights);
    }

    [Fact]
    public void Analyze_ReportsTimeOfDayPattern()
    {
        var events = CreateEvents(5, EventType.Smoked, hour: 18);

        var insight = Assert.Single(TriggerAnalyzer.Analyze(events).Insights,
            i => i.Kind == TriggerInsightKind.TimeOfDay);

        Assert.Equal(100, insight.Strength);
        Assert.Equal(5, insight.Count);
        Assert.Equal($"Trigger.TimeBucket.{GetLocalBucket(events[0])}", insight.Args!["bucket"]);
    }

    [Fact]
    public void Analyze_ReportsDayOfWeekPattern()
    {
        var events = CreateEvents(5, EventType.Craving, dayOffset: 2);

        var insight = Assert.Single(TriggerAnalyzer.Analyze(events).Insights,
            i => i.Kind == TriggerInsightKind.DayOfWeek);

        Assert.Equal(100, insight.Strength);
        Assert.Equal(5, insight.Count);
        Assert.Equal(events[0].OccurredAtUtc.LocalDateTime.DayOfWeek, insight.Args!["day"]);
    }

    [Fact]
    public void Analyze_ReportsMostFrequentContext()
    {
        var events = CreateEvents(5);
        events[0] = WithContext(events[0], "work");
        events[1] = WithContext(events[1], "work");
        events[2] = WithContext(events[2], "work");
        events[3] = WithContext(events[3], "home");

        var insight = Assert.Single(TriggerAnalyzer.Analyze(events).Insights,
            i => i.Kind == TriggerInsightKind.Context);

        Assert.Equal(75, insight.Strength);
        Assert.Equal(3, insight.Count);
        Assert.Equal("work", insight.Args!["context"]);
    }

    [Fact]
    public void Analyze_ReportsSmokedVersusResistedRatio()
    {
        var events = CreateEvents(5);
        events[0] = WithType(events[0], EventType.Smoked);
        events[1] = WithType(events[1], EventType.Smoked);
        events[2] = WithType(events[2], EventType.Smoked);
        events[3] = WithType(events[3], EventType.Resisted);
        events[4] = WithType(events[4], EventType.Resisted);

        var insight = Assert.Single(TriggerAnalyzer.Analyze(events).Insights,
            i => i.Kind == TriggerInsightKind.SmokedVsResisted);

        Assert.Equal(40, insight.Strength);
        Assert.Equal(5, insight.Count);
        Assert.Equal(3, insight.Args!["smoked"]);
        Assert.Equal(2, insight.Args["resisted"]);
    }

    [Fact]
    public void Analyze_WithSameInput_ReturnsDeterministicOutput()
    {
        var events = CreateEvents(8);

        var first = TriggerAnalyzer.Analyze(events);
        var second = TriggerAnalyzer.Analyze(events);

        Assert.Equal(first.IsEnabled, second.IsEnabled);
        Assert.Equal(first.HasSufficientData, second.HasSufficientData);
        Assert.Equal(first.TotalEventsAnalyzed, second.TotalEventsAnalyzed);
        Assert.Equal(first.Insights.Count, second.Insights.Count);
        Assert.Equal(first.Insights.Select(i => i.Kind), second.Insights.Select(i => i.Kind));
        Assert.Equal(
            first.Insights.Select(i => (i.LabelKey, i.Strength, i.Count, i.IsApproximate, i.Args?.OrderBy(p => p.Key).ToArray())),
            second.Insights.Select(i => (i.LabelKey, i.Strength, i.Count, i.IsApproximate, i.Args?.OrderBy(p => p.Key).ToArray())));
    }

    [Fact]
    public void Analyze_ReturnsOnlyAggregatedInsights()
    {
        var events = CreateEvents(5);
        events[0] = WithContext(events[0], "private-context");

        var result = TriggerAnalyzer.Analyze(events);
        var output = string.Join("|", result.Insights.Select(i => i.ToString()));

        foreach (var logEvent in events)
        {
            Assert.DoesNotContain(logEvent.EventId, output);
            Assert.DoesNotContain(logEvent.OccurredAtUtc.ToString("O"), output);
        }

        Assert.All(result.Insights, insight =>
        {
            Assert.InRange(insight.Strength, 0, 100);
            Assert.True(insight.Count >= 0);
            Assert.True(insight.IsApproximate);
        });
    }

    private static List<LogEvent> CreateEvents(
        int count,
        EventType type = EventType.Craving,
        int hour = 10,
        int dayOffset = 0)
    {
        var localBase = new DateTime(2024, 1, 8 + dayOffset, hour, 0, 0, DateTimeKind.Unspecified);
        var baseTime = new DateTimeOffset(localBase, TimeZoneInfo.Local.GetUtcOffset(localBase));

        return Enumerable.Range(0, count)
            .Select(index => new LogEvent(
                $"event-{index}",
                baseTime.AddMinutes(index),
                EventSource.Mobile,
                type,
                SyncStatus.Pending))
            .ToList();
    }

    private static LogEvent WithContext(LogEvent logEvent, string context)
        => new(logEvent.EventId, logEvent.OccurredAtUtc, logEvent.Source, logEvent.Type,
            logEvent.SyncStatus, new Dictionary<string, string> { ["context"] = context });

    private static LogEvent WithType(LogEvent logEvent, EventType type)
        => new(logEvent.EventId, logEvent.OccurredAtUtc, logEvent.Source, type, logEvent.SyncStatus,
            logEvent.Metadata);

    private static TimeBucket GetLocalBucket(LogEvent logEvent)
    {
        var hour = logEvent.OccurredAtUtc.LocalDateTime.Hour;
        return hour switch
        {
            >= 5 and < 8 => TimeBucket.EarlyMorning,
            >= 8 and < 12 => TimeBucket.Morning,
            >= 12 and < 17 => TimeBucket.Afternoon,
            >= 17 and < 21 => TimeBucket.Evening,
            _ => TimeBucket.Night,
        };
    }
}
