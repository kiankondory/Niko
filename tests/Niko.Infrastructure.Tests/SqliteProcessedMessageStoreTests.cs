// ============================================================================
// Niko.Infrastructure.Tests — SqliteProcessedMessageStoreTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون ماندگاری و idempotency ذخیره‌ساز پیام‌های Companion در SQLite.
// وابستگی‌ها و لایه: تست یکپارچهٔ Infrastructure → Core contract و SQLite.
// نکات تغییر و قیود: پایگاه‌دادهٔ موقت استفاده می‌شود؛ دادهٔ رویداد نیز برای
// اثبات عدم حذف بررسی می‌شود.
// ============================================================================

using Niko.Core.Events;
using Niko.Infrastructure.Persistence;

namespace Niko.Infrastructure.Tests;

public sealed class SqliteProcessedMessageStoreTests
{
    [Fact]
    public async Task MessageId_IsRejectedAfterStoreRecreation()
    {
        var path = Path.Combine(Path.GetTempPath(), $"niko_companion_{Guid.NewGuid():N}.db");
        var first = new SqliteProcessedMessageStore(path);

        Assert.True(await first.TryMarkProcessedAsync("message-1"));

        var second = new SqliteProcessedMessageStore(path);

        Assert.False(await second.TryMarkProcessedAsync("message-1"));
        Assert.True(await second.TryMarkProcessedAsync("message-2"));
    }

    [Fact]
    public async Task CompanionTable_DoesNotRemoveExistingEvents()
    {
        var path = Path.Combine(Path.GetTempPath(), $"niko_companion_{Guid.NewGuid():N}.db");
        var events = new SqliteStore(path);
        await events.SaveEventAsync(new LogEvent(
            "event-1",
            DateTimeOffset.UtcNow,
            EventSource.Mobile,
            EventType.Smoked,
            SyncStatus.Pending));

        var processed = new SqliteProcessedMessageStore(path);
        Assert.True(await processed.TryMarkProcessedAsync("message-1"));

        var saved = await events.GetEventsAsync();
        Assert.Single(saved);
        Assert.Equal("event-1", saved[0].EventId);
    }
}
