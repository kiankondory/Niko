// ============================================================================
// Niko.Core — QuickLogUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: مورد کاربرد ثبت سریع (یک‌لمسه) رویدادهای دود، مقاومت و هوس.
//           منطق دامنه را اعتبارسنجی کرده و رویداد را ابتدا به‌صورت محلی ذخیره
//           می‌کند تا رفتاری کاملاً آفلاین داشته باشد.
// وابستگی‌ها و لایه: UseCases → Abstractions (IClock, ILocalStore) در Core.
// نکات تغییر و قیود: فقط سه نوع اصلی (Smoked/Resisted/Craving) مجازند. ذخیرهٔ
//           محلی مقدم بر هر همگام‌سازی است (offline-first). EventId به‌صورت یکتا
//           تولید می‌شود و در صورت تکراری بودن ذخیره، idempotent است.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;

namespace Niko.Core.UseCases.QuickLog;

/// <summary>
/// مورد کاربرد ثبت سریع رویداد. ورودی را اعتبارسنجی کرده و به‌صورت محلی ذخیره می‌کند.
/// </summary>
public sealed class QuickLogUseCase
{
    private static readonly HashSet<EventType> _allowedTypes = new()
    {
        EventType.Smoked,
        EventType.Resisted,
        EventType.Craving,
    };

    private readonly ILocalStore _store;
    private readonly IClock _clock;

    public QuickLogUseCase(ILocalStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    public async Task<QuickLogResult> ExecuteAsync(
        QuickLogRequest request,
        CancellationToken ct = default)
    {
        if (!_allowedTypes.Contains(request.Type))
        {
            throw new ArgumentException(
                $"نوع رویداد {request.Type} برای ثبت سریع مجاز نیست.",
                nameof(request));
        }

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        if (request.Intensity is { } intensity)
        {
            metadata["intensity"] = intensity.ToString();
        }

        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            metadata["context"] = request.Context!;
        }

        var occurredAtUtc = request.OccurredAtUtc ?? _clock.UtcNow;
        var logEvent = new LogEvent(
            eventId: string.IsNullOrWhiteSpace(request.EventId)
                ? Guid.NewGuid().ToString("N")
                : request.EventId,
            occurredAtUtc: occurredAtUtc,
            source: request.Source,
            type: request.Type,
            syncStatus: SyncStatus.Pending,
            metadata: metadata);

        await _store.SaveEventAsync(logEvent, ct).ConfigureAwait(false);

        return new QuickLogResult(logEvent.EventId, logEvent.Type, logEvent.SyncStatus);
    }
}
