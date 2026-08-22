// ============================================================================
// Niko.Core.Tests — SyncQueueTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های صف همگام‌سازی: موفقیت، شکست، backoff نمایی، و عدم ارسال
//           رویدادهای InSync.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و از FakeClock برای کنترل زمان backoff استفاده می‌کنند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;
using Niko.Core.Sync;

namespace Niko.Core.Tests;

public class SyncQueueTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryStore _store;
    private readonly FakeTransport _transport;
    private readonly SyncQueue _queue;

    public SyncQueueTests()
    {
        _clock = new FakeClock();
        _store = new InMemoryStore();
        _transport = new FakeTransport();
        _queue = new SyncQueue(_store, _transport, _clock);
    }

    private async Task SeedAsync(string id, SyncStatus status = SyncStatus.Pending)
    {
        await _store.SaveEventAsync(new LogEvent(
            id,
            _clock.UtcNow,
            EventSource.Mobile,
            EventType.Smoked,
            status));
    }

    [Fact]
    public async Task RunOnce_WithNoPending_DoesNothing()
    {
        var result = await _queue.RunOnceAsync();

        Assert.Equal(0, result.PendingCount);
        Assert.Equal(0, _transport.PushCount);
    }

    [Fact]
    public async Task RunOnce_WithPending_MarksAcceptedInSync()
    {
        await SeedAsync("a");
        await SeedAsync("b");

        var result = await _queue.RunOnceAsync();

        Assert.Equal(2, result.Accepted);
        Assert.All(_store.Events, e => Assert.Equal(SyncStatus.InSync, e.SyncStatus));
    }

    [Fact]
    public async Task RunOnce_DoesNotResendInSyncEvents()
    {
        await SeedAsync("a", SyncStatus.InSync);

        var result = await _queue.RunOnceAsync();

        Assert.Equal(0, result.PendingCount);
        Assert.Equal(0, _transport.PushCount);
    }

    [Fact]
    public async Task RunOnce_WithFailedEvent_MarksFailedAndAppliesBackoff()
    {
        var transport = new FakeTransport("bad-1");
        var queue = new SyncQueue(_store, transport, _clock);
        await SeedAsync("bad-1");

        var first = await queue.RunOnceAsync();
        Assert.Equal(1, first.Failed);
        Assert.Equal(SyncStatus.Failed, _store.Events[0].SyncStatus);

        // قبل از سپری شدن backoff، ارسال مجدد نباید رخ دهد.
        var immediate = await queue.RunOnceAsync();
        Assert.Equal(0, immediate.Accepted);
        Assert.Equal(0, immediate.Failed);

        // پس از سپری شدن backoff (1 ثانیه)، تلاش مجدد رخ می‌دهد.
        _clock.UtcNow = _clock.UtcNow.AddSeconds(2);
        var retried = await queue.RunOnceAsync();
        Assert.Equal(1, retried.Accepted);
        Assert.Equal(SyncStatus.InSync, _store.Events[0].SyncStatus);
    }

    [Fact]
    public async Task RunOnce_WhenTransportAlwaysFails_RepeatedAttemptsAreBackedOff()
    {
        var transport = new FakeTransport(alwaysFail: true);
        var queue = new SyncQueue(_store, transport, _clock);
        await SeedAsync("x");

        await queue.RunOnceAsync();
        _clock.UtcNow = _clock.UtcNow.AddSeconds(2);

        // try 1 (after 1s backoff) -> fails again
        await queue.RunOnceAsync();
        _clock.UtcNow = _clock.UtcNow.AddSeconds(4);

        // try 2 (after 2s backoff) -> fails again
        await queue.RunOnceAsync();

        Assert.Equal(3, transport.PushCount);
        Assert.Equal(SyncStatus.Failed, _store.Events[0].SyncStatus);
    }

    [Fact]
    public async Task RunOnce_WithMixedSuccessAndFailure_UpdatesEach()
    {
        var transport = new FakeTransport("fail-1");
        var queue = new SyncQueue(_store, transport, _clock);
        await SeedAsync("ok-1");
        await SeedAsync("fail-1");

        var result = await queue.RunOnceAsync();

        Assert.Equal(1, result.Accepted);
        Assert.Equal(1, result.Failed);
        Assert.Equal(SyncStatus.InSync, _store.Events.Single(e => e.EventId == "ok-1").SyncStatus);
        Assert.Equal(SyncStatus.Failed, _store.Events.Single(e => e.EventId == "fail-1").SyncStatus);
    }
}
