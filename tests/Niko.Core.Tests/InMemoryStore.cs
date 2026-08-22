// ============================================================================
// Niko.Core.Tests — InMemoryStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی درون‌حافظه‌ای ILocalStore برای تست منطق دامنه بدون نیاز به
//           ذخیره‌ساز واقعی. رفتار idempotency را برای شناسهٔ تکراری شبیه می‌کند.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای تست؛ نباید در کد تولید استفاده شود.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;

namespace Niko.Core.Tests;

/// <summary>
/// ذخیره‌ساز درون‌حافظهٔ تستی.
/// </summary>
public sealed class InMemoryStore : ILocalStore
{
    private readonly List<LogEvent> _events = new();

    public IReadOnlyList<LogEvent> Events => _events;

    public Task SaveEventAsync(LogEvent logEvent, CancellationToken ct = default)
    {
        if (_events.Any(e => e.EventId == logEvent.EventId))
        {
            return Task.CompletedTask;
        }

        _events.Add(logEvent);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<LogEvent>> GetEventsAsync(
        int offset = 0,
        int limit = 100,
        CancellationToken ct = default)
    {
        var result = _events
            .OrderBy(e => e.OccurredAtUtc)
            .Skip(offset)
            .Take(limit)
            .ToList();
        return Task.FromResult<IReadOnlyList<LogEvent>>(result);
    }

    public Task<IReadOnlyList<LogEvent>> GetPendingEventsAsync(
        int limit = 100,
        CancellationToken ct = default)
    {
        // رویدادهای در انتظار همگام‌سازی: هم وضعیت Pending و هم Failed
        // (برای تلاش مجدد با backoff) مشمول این دسته‌اند.
        var result = _events
            .Where(e => e.SyncStatus != SyncStatus.InSync)
            .Take(limit)
            .ToList();
        return Task.FromResult<IReadOnlyList<LogEvent>>(result);
    }

    public Task UpdateSyncStatusAsync(
        string eventId,
        SyncStatus status,
        CancellationToken ct = default)
    {
        var index = _events.FindIndex(e => e.EventId == eventId);
        if (index >= 0)
        {
            var original = _events[index];
            _events[index] = new LogEvent(
                original.EventId,
                original.OccurredAtUtc,
                original.Source,
                original.Type,
                status,
                original.Metadata);
        }

        return Task.CompletedTask;
    }
}
