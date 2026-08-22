// ============================================================================
// Niko.Core.Tests — CompanionMessageSerializerTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های سریال‌کنندهٔ پیام ابزارک/ساعت: round-trip، JSON نامعتبر و
//           بررسی نسخهٔ قرارداد.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و CompanionMessageSerializer را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند.
// ============================================================================

using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public class CompanionMessageSerializerTests
{
    [Fact]
    public void RoundTrip_Deserialize_ReturnsSameFields()
    {
        var json = CompanionMessageSerializer.Serialize(new CompanionMessage
        {
            ContractVersion = 1,
            MessageId = "m1",
            Source = EventSource.Wearable,
            MessageType = CompanionMessageType.QuickLog,
            Payload = "{}",
            SentAtUtc = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        });

        var message = CompanionMessageSerializer.DeserializeMessage(json);

        Assert.NotNull(message);
        Assert.Equal(1, message.ContractVersion);
        Assert.Equal("m1", message.MessageId);
        Assert.Equal(EventSource.Wearable, message.Source);
        Assert.Equal(CompanionMessageType.QuickLog, message.MessageType);
    }

    [Fact]
    public void Deserialize_EmptyVersion_DefaultsToCurrent()
    {
        var json = "{\"messageId\":\"m2\",\"source\":3,\"messageType\":0,\"payload\":\"{}\"}";
        var message = CompanionMessageSerializer.DeserializeMessage(json);

        Assert.NotNull(message);
        Assert.Equal(CompanionMessageSerializer.CurrentContractVersion, message.ContractVersion);
    }

    [Fact]
    public void Deserialize_MalformedJson_ReturnsNull()
    {
        Assert.Null(CompanionMessageSerializer.DeserializeMessage("not json {{{"));
        Assert.Null(CompanionMessageSerializer.DeserializeMessage(""));
    }

    [Fact]
    public void IsVersionSupported_CurrentYes_OthersNo()
    {
        Assert.True(CompanionMessageSerializer.IsVersionSupported(1));
        Assert.False(CompanionMessageSerializer.IsVersionSupported(2));
        Assert.False(CompanionMessageSerializer.IsVersionSupported(0));
    }

    [Fact]
    public void DeserializePayload_RoundTripsTypedPayload()
    {
        var payload = new CompanionQuickLogRequest
        {
            EventType = EventType.Smoked,
            Intensity = 5,
            Context = "stress",
        };

        var json = CompanionMessageSerializer.Serialize(payload);
        var back = CompanionMessageSerializer.DeserializePayload<CompanionQuickLogRequest>(json);

        Assert.NotNull(back);
        Assert.Equal(EventType.Smoked, back.EventType);
        Assert.Equal(5, back.Intensity);
        Assert.Equal("stress", back.Context);
    }
}
