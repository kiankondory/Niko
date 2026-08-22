// ============================================================================
// Niko.Core.Tests — FakeTransport.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی شبیه‌سازی‌شدهٔ ISyncTransport برای تست صف همگام‌سازی.
//           امکان شبیه‌سازی موفقیت/شکست دسته‌ای و شبکهٔ ناموفق را فراهم می‌کند.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای تست؛ بدون شبکهٔ واقعی.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;

namespace Niko.Core.Tests;

/// <summary>
/// انتقال شبیه‌سازی‌شده که می‌تواند موفق یا ناموفق عمل کند.
/// </summary>
public sealed class FakeTransport : ISyncTransport
{
    private readonly HashSet<string>? _failEventIds;
    private readonly Dictionary<string, int> _failRemaining = new(StringComparer.Ordinal);

    public FakeTransport(bool alwaysFail = false)
    {
        AlwaysFail = alwaysFail;
    }

    public FakeTransport(params string[] failEventIds)
    {
        _failEventIds = new HashSet<string>(failEventIds, StringComparer.Ordinal);
        foreach (var id in failEventIds)
        {
            _failRemaining[id] = 1;
        }
    }

    public bool AlwaysFail { get; }

    public int PushCount { get; private set; }

    public List<IReadOnlyList<LogEvent>> PushedBatches { get; } = new();

    public Task<SyncResult> PushAsync(
        IReadOnlyList<LogEvent> events,
        CancellationToken ct = default)
    {
        PushCount++;
        PushedBatches.Add(events);

        if (AlwaysFail)
        {
            return Task.FromResult(new SyncResult(
                Array.Empty<string>(),
                events.Select(e => e.EventId).ToArray()));
        }

        var accepted = new List<string>();
        var failed = new List<string>();

        foreach (var evt in events)
        {
            // رویداد مشخص‌شده فقط تعداد محدودی بار شکست می‌خورد تا رفتار
            // «شکست سپس موفقیت» برای تست backoff شبیه‌سازی شود.
            if (_failRemaining.TryGetValue(evt.EventId, out var remaining) && remaining > 0)
            {
                _failRemaining[evt.EventId] = remaining - 1;
                failed.Add(evt.EventId);
            }
            else
            {
                accepted.Add(evt.EventId);
            }
        }

        return Task.FromResult(new SyncResult(accepted, failed));
    }
}
