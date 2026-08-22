// ============================================================================
// Niko.Core.Tests — CompanionWidgetContractTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون قراردادهای قابل استفادهٔ ابزارک برای QuickLog و خلاصه‌های امن.
// وابستگی‌ها و لایه: تست Core → Companion contracts؛ بدون Android یا شبکه.
// نکات تغییر و قیود: ورودی‌ها قطعی، نسخه‌بندی‌شده و بدون دادهٔ خصوصی هستند.
// ============================================================================

using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public sealed class CompanionWidgetContractTests
{
    [Theory]
    [InlineData(EventType.Smoked)]
    [InlineData(EventType.Resisted)]
    [InlineData(EventType.Craving)]
    public void WidgetQuickLogMessage_RoundTrips(EventType type)
    {
        var message = new CompanionMessage
        {
            ContractVersion = CompanionMessageSerializer.CurrentContractVersion,
            MessageId = "widget-message",
            Source = EventSource.Widget,
            MessageType = CompanionMessageType.QuickLog,
            Payload = CompanionMessageSerializer.Serialize(new CompanionQuickLogRequest { EventType = type }),
            SentAtUtc = new DateTimeOffset(2026, 8, 21, 0, 0, 0, TimeSpan.Zero),
        };

        var restored = CompanionMessageSerializer.DeserializeMessage(
            CompanionMessageSerializer.Serialize(message));
        Assert.NotNull(restored);
        var payload = restored!.Payload;
        var request = CompanionMessageSerializer.DeserializePayload<CompanionQuickLogRequest>(payload);

        Assert.Equal(EventSource.Widget, restored.Source);
        Assert.Equal(CompanionMessageType.QuickLog, restored.MessageType);
        Assert.NotNull(request);
        Assert.Equal(type, request!.EventType);
    }

    [Fact]
    public void UnsupportedVersion_IsRejected()
    {
        Assert.False(CompanionMessageSerializer.IsVersionSupported(
            CompanionMessageSerializer.CurrentContractVersion + 1));
    }

    [Fact]
    public void SummaryContracts_ContainOnlyAggregates()
    {
        var progress = new CompanionProgressSummary(1, 2, 3, 25, false);
        var streak = new CompanionStreakSummary(4, 7, 14);
        var sync = new CompanionSyncStatusSummary(1, false);

        Assert.Equal(1, progress.TotalSmoked);
        Assert.Equal(0, progress.SmokedToday);
        Assert.Equal(4, streak.CurrentStreakDays);
        Assert.False(sync.InSync);
    }
}
