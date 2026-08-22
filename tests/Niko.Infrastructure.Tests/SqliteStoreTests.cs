// ============================================================================
// Niko.Infrastructure.Tests — SqliteStoreTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های یکپارچهٔ ذخیره‌ساز SQLite: ذخیره/بازیابی، idempotency، وضعیت
//           همگام‌سازی، و رفتار آفلاین (بدون شبکه). از پایگاه‌دادهٔ موقت استفاده می‌کند.
// وابستگی‌ها و لایه: لایهٔ تست؛ Infrastructure و Core را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها از پایگاه‌دادهٔ موقت هر اجرا استفاده می‌کنند و به
//           شبکه وابسته نیستند.
// ============================================================================

using Niko.Core.Events;
using Niko.Infrastructure.Persistence;

namespace Niko.Infrastructure.Tests;

public class SqliteStoreTests
{
    private static string NewTempPath()
    {
        return Path.Combine(Path.GetTempPath(), $"niko_{Guid.NewGuid():N}.db");
    }

    private static LogEvent CreateEvent(
        string id,
        EventType type = EventType.Smoked,
        SyncStatus sync = SyncStatus.Pending,
        DateTimeOffset? occurredAtUtc = null)
    {
        return new LogEvent(
            id,
            occurredAtUtc ?? new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EventSource.Mobile,
            type,
            sync,
            new Dictionary<string, string> { ["intensity"] = "5" });
    }

    [Fact]
    public async Task SaveThenGet_ReturnsSameEvent()
    {
        var path = NewTempPath();
        var store = new SqliteStore(path);
        var evt = CreateEvent("id-1");

        await store.SaveEventAsync(evt);

        var events = await store.GetEventsAsync();
        var saved = Assert.Single(events);
        Assert.Equal("id-1", saved.EventId);
        Assert.Equal(EventType.Smoked, saved.Type);
        Assert.Equal(SyncStatus.Pending, saved.SyncStatus);
        Assert.Equal("5", saved.Metadata["intensity"]);
    }

    [Fact]
    public async Task SaveSameId_IsIdempotent()
    {
        var store = new SqliteStore(NewTempPath());
        var evt = CreateEvent("dup-id");

        await store.SaveEventAsync(evt);
        await store.SaveEventAsync(evt);

        var events = await store.GetEventsAsync();
        Assert.Single(events);
    }

    [Fact]
    public async Task GetEvents_OrdersByOccurredAt()
    {
        var store = new SqliteStore(NewTempPath());
        await store.SaveEventAsync(CreateEvent("b", occurredAtUtc: new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero)));
        await store.SaveEventAsync(CreateEvent("a", occurredAtUtc: new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero)));

        var events = await store.GetEventsAsync();

        Assert.Equal(new[] { "a", "b" }, events.Select(e => e.EventId));
    }

    [Fact]
    public async Task UpdateSyncStatus_ReflectsNewStatus()
    {
        var store = new SqliteStore(NewTempPath());
        var evt = CreateEvent("sync-1");
        await store.SaveEventAsync(evt);

        await store.UpdateSyncStatusAsync("sync-1", SyncStatus.InSync);

        var events = await store.GetEventsAsync();
        Assert.Equal(SyncStatus.InSync, events[0].SyncStatus);
    }

    [Fact]
    public async Task GetPendingEvents_ReturnsPendingAndFailed_NotInSync()
    {
        var store = new SqliteStore(NewTempPath());
        await store.SaveEventAsync(CreateEvent("p1", sync: SyncStatus.Pending));
        await store.SaveEventAsync(CreateEvent("s1", sync: SyncStatus.InSync));
        await store.SaveEventAsync(CreateEvent("f1", sync: SyncStatus.Failed));

        var pending = await store.GetPendingEventsAsync();

        Assert.Equal(
            new[] { "f1", "p1" },
            pending.Select(e => e.EventId).OrderBy(x => x));
    }

    [Fact]
    public async Task PersistsAcrossNewStoreInstance_SameFile()
    {
        var path = NewTempPath();
        var store1 = new SqliteStore(path);
        await store1.SaveEventAsync(CreateEvent("persist-1"));

        var store2 = new SqliteStore(path);
        var events = await store2.GetEventsAsync();

        Assert.Single(events);
        Assert.Equal("persist-1", events[0].EventId);
    }
}
