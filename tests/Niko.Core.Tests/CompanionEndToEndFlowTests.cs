// نام فایل: CompanionEndToEndFlowTests.cs
// مسئولیت: آزمون مسیر کامل پیام companion از serialization تا رویداد محلی و خلاصهٔ aggregate.
// وابستگی‌ها و لایه: تست Core → CompanionUseCase، QuickLog و Dashboard؛ بدون Android، Wear SDK، شبکه یا SQLite واقعی.
// نکات تغییر و قیود: مسیرهای Widget و Wear باید یک منبع حقیقت مشترک و idempotency یکسان داشته باشند.

using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;
using Niko.Core.UseCases.Companion;
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.QuickLog;

namespace Niko.Core.Tests;

public sealed class CompanionEndToEndFlowTests
{
    [Fact]
    public async Task WidgetAndWearMessagesShareCorePersistenceAndAggregateSummary()
    {
        var clock = new FakeClock
        {
            UtcNow = new DateTimeOffset(2026, 8, 22, 10, 0, 0, TimeSpan.Zero),
        };
        var store = new InMemoryStore();
        var processed = new TestProcessedMessageStore();
        var useCase = new CompanionUseCase(
            new QuickLogUseCase(store, clock),
            new DashboardUseCase(store, new InMemoryUserSettingsStore(), clock),
            processed,
            store,
            clock,
            TimeZoneInfo.Utc);

        var widget = await useCase.HandleAsync(CreateMessage(
            "widget-message",
            "widget-event",
            EventSource.Widget,
            EventType.Smoked));
        var wear = await useCase.HandleAsync(CreateMessage(
            "wear-message",
            "wear-event",
            EventSource.Wearable,
            EventType.Resisted));

        Assert.True(widget.Success);
        Assert.True(wear.Success);
        Assert.Equal(2, store.Events.Count);
        Assert.Contains(store.Events, e => e.Source == EventSource.Widget && e.Type == EventType.Smoked);
        Assert.Contains(store.Events, e => e.Source == EventSource.Wearable && e.Type == EventType.Resisted);

        var summaryResult = await useCase.HandleAsync(CreateRequest(
            "summary-message",
            CompanionMessageType.ProgressSummaryRequest));
        var summary = Assert.IsType<CompanionProgressSummary>(summaryResult.Data);

        Assert.Equal(1, summary.SmokedToday);
        Assert.Equal(1, summary.ResistedToday);
        Assert.DoesNotContain("widget-event", summary.ToString());
        Assert.DoesNotContain("wear-event", summary.ToString());
    }

    private static string CreateMessage(
        string messageId,
        string eventId,
        EventSource source,
        EventType type)
    {
        var message = new CompanionMessage
        {
            ContractVersion = CompanionMessageSerializer.CurrentContractVersion,
            MessageId = messageId,
            Source = source,
            MessageType = CompanionMessageType.QuickLog,
            Payload = CompanionMessageSerializer.Serialize(new CompanionQuickLogRequest
            {
                EventType = type,
                EventId = eventId,
            }),
            SentAtUtc = new DateTimeOffset(2026, 8, 22, 10, 0, 0, TimeSpan.Zero),
        };

        return CompanionMessageSerializer.Serialize(message);
    }

    private static string CreateRequest(string messageId, CompanionMessageType type)
    {
        var message = new CompanionMessage
        {
            ContractVersion = CompanionMessageSerializer.CurrentContractVersion,
            MessageId = messageId,
            Source = EventSource.Widget,
            MessageType = type,
            Payload = "{}",
        };

        return CompanionMessageSerializer.Serialize(message);
    }
}
