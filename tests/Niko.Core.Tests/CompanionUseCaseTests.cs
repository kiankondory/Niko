// ============================================================================
// Niko.Core.Tests — CompanionUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های مورد کاربرد پیام ابزارک/ساعت: انواع QuickLog، اعتبارسنجی منبع،
//           نسخهٔ قرارداد، پیام تکراری، محتوای نامعتبر و نگاشت وضعیت همگام‌سازی.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و از FakeClock استفاده می‌کنند.
// ============================================================================

using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;
using Niko.Core.UseCases.Companion;
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.QuickLog;

namespace Niko.Core.Tests;

public class CompanionUseCaseTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryStore _store;
    private readonly InMemoryUserSettingsStore _settings;
    private readonly TestProcessedMessageStore _processed;
    private readonly CompanionUseCase _useCase;

    public CompanionUseCaseTests()
    {
        _clock = new FakeClock { UtcNow = new DateTimeOffset(2024, 3, 1, 12, 0, 0, TimeSpan.Zero) };
        _store = new InMemoryStore();
        _settings = new InMemoryUserSettingsStore();
        _processed = new TestProcessedMessageStore();

        var quickLog = new QuickLogUseCase(_store, _clock);
        var dashboard = new DashboardUseCase(_store, _settings, _clock);
        _useCase = new CompanionUseCase(quickLog, dashboard, _processed, _store, _clock, TimeZoneInfo.Utc);
    }

    private static string QuickLogMessage(
        EventType type,
        string? messageId = null,
        string? eventId = null,
        EventSource source = EventSource.Widget,
        int version = 1)
    {
        var message = new CompanionMessage
        {
            ContractVersion = version,
            MessageId = messageId ?? Guid.NewGuid().ToString("N"),
            Source = source,
            MessageType = CompanionMessageType.QuickLog,
            Payload = CompanionMessageSerializer.Serialize(new CompanionQuickLogRequest
            {
                EventType = type,
                EventId = eventId,
            }),
            SentAtUtc = new DateTimeOffset(2024, 3, 1, 12, 0, 0, TimeSpan.Zero),
        };

        return CompanionMessageSerializer.Serialize(message);
    }

    [Theory]
    [InlineData(EventType.Smoked)]
    [InlineData(EventType.Resisted)]
    [InlineData(EventType.Craving)]
    public async Task QuickLog_AllEventTypes_Succeed(EventType type)
    {
        var result = await _useCase.HandleAsync(QuickLogMessage(type));

        Assert.True(result.Success);
        Assert.Equal(CompanionErrorCode.None, result.ErrorCode);
        var response = Assert.IsType<CompanionQuickLogResponse>(result.Data);
        Assert.True(response.Success);
        Assert.Equal(SyncStatus.Pending, response.SyncStatus);
        Assert.Equal(type, _store.Events.Single().Type);
    }

    [Fact]
    public async Task QuickLog_InvalidSource_ReturnsInvalidSource()
    {
        var result = await _useCase.HandleAsync(
            QuickLogMessage(EventType.Smoked, source: (EventSource)99));

        Assert.False(result.Success);
        Assert.Equal(CompanionErrorCode.InvalidSource, result.ErrorCode);
    }

    [Fact]
    public async Task UnsupportedVersion_ReturnsUnsupportedVersion()
    {
        var result = await _useCase.HandleAsync(QuickLogMessage(EventType.Smoked, version: 2));

        Assert.False(result.Success);
        Assert.Equal(CompanionErrorCode.UnsupportedVersion, result.ErrorCode);
    }

    [Fact]
    public async Task MalformedJson_ReturnsMalformedPayload()
    {
        var result = await _useCase.HandleAsync("not-json");

        Assert.False(result.Success);
        Assert.Equal(CompanionErrorCode.MalformedPayload, result.ErrorCode);
    }

    [Fact]
    public async Task DuplicateMessageId_ReturnsDuplicateEvent()
    {
        var msg = QuickLogMessage(EventType.Smoked, messageId: "dup-id");
        await _useCase.HandleAsync(msg);
        var second = await _useCase.HandleAsync(msg);

        Assert.False(second.Success);
        Assert.Equal(CompanionErrorCode.DuplicateEvent, second.ErrorCode);
        // فقط یک رویداد ذخیره شده است (idempotent).
        Assert.Single(_store.Events);
    }

    [Fact]
    public async Task QuickLog_DisallowedEventType_ReturnsValidationFailed()
    {
        var msg = QuickLogMessage(EventType.CravingAction);
        var result = await _useCase.HandleAsync(msg);

        Assert.False(result.Success);
        Assert.Equal(CompanionErrorCode.ValidationFailed, result.ErrorCode);
    }

    [Fact]
    public async Task ProgressSummary_ReturnsCountsAndMilestone()
    {
        await _store.SaveEventAsync(new LogEvent("e1", _clock.UtcNow, EventSource.Mobile, EventType.Smoked, SyncStatus.Pending));

        var msg = BuildRequest(CompanionMessageType.ProgressSummaryRequest);
        var result = await _useCase.HandleAsync(msg);

        Assert.True(result.Success);
        var summary = Assert.IsType<CompanionProgressSummary>(result.Data);
        Assert.Equal(1, summary.TotalSmoked);
    }

    [Fact]
    public async Task ProgressSummary_ReturnsCurrentLocalDayCounts()
    {
        await _store.SaveEventAsync(new LogEvent("today-smoked", _clock.UtcNow, EventSource.Mobile, EventType.Smoked, SyncStatus.Pending));
        await _store.SaveEventAsync(new LogEvent("today-resisted", _clock.UtcNow, EventSource.Mobile, EventType.Resisted, SyncStatus.Pending));
        await _store.SaveEventAsync(new LogEvent("future", _clock.UtcNow.AddMinutes(1), EventSource.Mobile, EventType.Smoked, SyncStatus.Pending));

        var result = await _useCase.HandleAsync(BuildRequest(CompanionMessageType.ProgressSummaryRequest));

        var summary = Assert.IsType<CompanionProgressSummary>(result.Data);
        Assert.Equal(1, summary.SmokedToday);
        Assert.Equal(1, summary.ResistedToday);
    }

    [Fact]
    public async Task ProgressSummary_DuplicateQuickLogMessageDoesNotIncrementCountTwice()
    {
        var message = QuickLogMessage(EventType.Smoked, messageId: "widget-smoked-once");

        Assert.True((await _useCase.HandleAsync(message)).Success);
        Assert.False((await _useCase.HandleAsync(message)).Success);

        var result = await _useCase.HandleAsync(BuildRequest(CompanionMessageType.ProgressSummaryRequest));
        var summary = Assert.IsType<CompanionProgressSummary>(result.Data);

        Assert.Equal(1, summary.SmokedToday);
    }

    [Fact]
    public async Task ProgressSummary_DuplicateQuickLogEventIdDoesNotIncrementCountTwice()
    {
        var first = QuickLogMessage(EventType.Smoked, messageId: "widget-message-1", eventId: "widget-event-1");
        var second = QuickLogMessage(EventType.Smoked, messageId: "widget-message-2", eventId: "widget-event-1");

        Assert.True((await _useCase.HandleAsync(first)).Success);
        Assert.True((await _useCase.HandleAsync(second)).Success);

        var result = await _useCase.HandleAsync(BuildRequest(CompanionMessageType.ProgressSummaryRequest));
        var summary = Assert.IsType<CompanionProgressSummary>(result.Data);

        Assert.Equal(1, summary.SmokedToday);
        Assert.Single(_store.Events);
    }

    [Fact]
    public async Task StreakSummary_ReturnsStreakAndMilestones()
    {
        await _store.SaveEventAsync(new LogEvent("e1", _clock.UtcNow, EventSource.Mobile, EventType.Resisted, SyncStatus.Pending));

        var msg = BuildRequest(CompanionMessageType.StreakSummaryRequest);
        var result = await _useCase.HandleAsync(msg);

        Assert.True(result.Success);
        var summary = Assert.IsType<CompanionStreakSummary>(result.Data);
        Assert.Equal(1, summary.CurrentStreakDays);
    }

    [Fact]
    public async Task SyncStatus_NoPending_ReportsInSync()
    {
        var msg = BuildRequest(CompanionMessageType.SyncStatusRequest);
        var result = await _useCase.HandleAsync(msg);

        Assert.True(result.Success);
        var summary = Assert.IsType<CompanionSyncStatusSummary>(result.Data);
        Assert.True(summary.InSync);
        Assert.Equal(0, summary.PendingCount);
    }

    [Fact]
    public async Task UnknownMessageType_ReturnsUnknownMessage()
    {
        var message = new CompanionMessage
        {
            ContractVersion = 1,
            MessageId = Guid.NewGuid().ToString("N"),
            Source = EventSource.Widget,
            MessageType = (CompanionMessageType)99,
            Payload = "{}",
        };
        var result = await _useCase.HandleAsync(CompanionMessageSerializer.Serialize(message));

        Assert.False(result.Success);
        Assert.Equal(CompanionErrorCode.UnknownMessage, result.ErrorCode);
    }

    private static string BuildRequest(CompanionMessageType type)
    {
        var message = new CompanionMessage
        {
            ContractVersion = 1,
            MessageId = Guid.NewGuid().ToString("N"),
            Source = EventSource.Widget,
            MessageType = type,
            Payload = "{}",
        };

        return CompanionMessageSerializer.Serialize(message);
    }
}
