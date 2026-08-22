// ============================================================================
// Niko.Core — SyncQueue.cs
// ----------------------------------------------------------------------------
// مسئولیت: تخلیهٔ صف رویدادهای در انتظار همگام‌سازی. رویدادهای محلی ذخیره‌شده را
//           از طریق ISyncTransport ارسال می‌کند، وضعیت موفق/ناموفق را به‌روزرسانی
//           می‌کند و برای شکست‌ها backoff نمایی با تلاش مجدد اعمال می‌کند.
// وابستگی‌ها و لایه: بخش Sync در Core → Abstractions (ILocalStore, ISyncTransport,
//           IClock). بدون وابستگی به پلتفرم یا شبکهٔ واقعی.
// نکات تغییر و قیود: همگام‌سازی idempotent است (سرویس بر پایهٔ EventId تکراری را
//           حذف می‌کند). فقط رویدادهای Pending ارسال می‌شوند. تلاش مجدد به‌صورت
//           درون‌حافظه است؛ پس از راه‌اندازی مجدد، رویدادهای Pending دوباره ارسال
//           می‌شوند که سازگار با idempotency است.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;

namespace Niko.Core.Sync;

/// <summary>
/// صف همگام‌سازی رویدادها با تلاش مجدد و backoff نمایی.
/// </summary>
public sealed class SyncQueue
{
    private readonly ILocalStore _store;
    private readonly ISyncTransport _transport;
    private readonly IClock _clock;
    private readonly Dictionary<string, (int Attempts, DateTimeOffset LastAttempt)> _state =
        new(StringComparer.Ordinal);

    public SyncQueue(ILocalStore store, ISyncTransport transport, IClock clock)
    {
        _store = store;
        _transport = transport;
        _clock = clock;
    }

    /// <summary>
    /// اجرای یک چرخهٔ همگام‌سازی: ارسال رویدادهای در انتظار و به‌روزرسانی وضعیت‌ها.
    /// </summary>
    public async Task<SyncRunResult> RunOnceAsync(
        int batchSize = 50,
        CancellationToken ct = default)
    {
        var pending = await _store.GetPendingEventsAsync(batchSize, ct).ConfigureAwait(false);
        var result = new SyncRunResult(pending.Count);

        if (pending.Count == 0)
        {
            return result;
        }

        var ready = pending
            .Where(e => IsBackoffElapsed(e.EventId))
            .ToList();

        if (ready.Count == 0)
        {
            return result;
        }

        var pushResult = await _transport.PushAsync(ready, ct).ConfigureAwait(false);
        var now = _clock.UtcNow;

        foreach (var id in pushResult.AcceptedEventIds)
        {
            await _store.UpdateSyncStatusAsync(id, SyncStatus.InSync, ct).ConfigureAwait(false);
            _state.Remove(id);
            result.Accepted++;
        }

        foreach (var id in pushResult.FailedEventIds)
        {
            await _store.UpdateSyncStatusAsync(id, SyncStatus.Failed, ct).ConfigureAwait(false);
            var current = _state.GetValueOrDefault(id);
            _state[id] = (current.Attempts + 1, now);
            result.Failed++;
        }

        return result;
    }

    private bool IsBackoffElapsed(string eventId)
    {
        if (!_state.TryGetValue(eventId, out var state) || state.Attempts == 0)
        {
            return true;
        }

        // بازگشت نمایی: 1s، 2s، 4s، 8s... (پایهٔ 2^attempts ثانیه)
        var delay = TimeSpan.FromSeconds(Math.Pow(2, state.Attempts - 1));
        return state.LastAttempt + delay <= _clock.UtcNow;
    }
}

/// <summary>
/// نتیجهٔ یک چرخهٔ همگام‌سازی.
/// </summary>
public sealed class SyncRunResult
{
    public SyncRunResult(int pendingCount)
    {
        PendingCount = pendingCount;
    }

    /// <summary>تعداد رویدادهای در انتظار در ابتدای چرخه.</summary>
    public int PendingCount { get; }

    /// <summary>تعداد رویدادهای موفق.</summary>
    public int Accepted { get; set; }

    /// <summary>تعداد رویدادهای ناموفق.</summary>
    public int Failed { get; set; }
}
