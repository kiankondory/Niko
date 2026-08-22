// ============================================================================
// Niko.App — NoopSyncTransport.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی موقت ISyncTransport که هیچ‌چیز واقعی ارسال نمی‌کند. تا
//           زمانی که سرویس همگام‌سازی واقعی تعریف شود، رویدادها در صف محلی می‌مانند
//           و رفتار آفلاین حفظ می‌شود.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI)؛ قرارداد Sync هسته را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای مرحلهٔ اولیه؛ با تعریف سرویس واقعی جایگزین می‌شود.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;

namespace Niko.Services;

/// <summary>
/// انتقال همگام‌سازی بدون عملیات (موقت).
/// </summary>
public sealed class NoopSyncTransport : ISyncTransport
{
    public Task<SyncResult> PushAsync(
        IReadOnlyList<LogEvent> events,
        CancellationToken ct = default)
    {
        // رویدادها پذیرفته نمی‌شوند و به‌صورت ناموفق بازمی‌گردند تا در صف محلی بمانند.
        return Task.FromResult(new SyncResult(
            Array.Empty<string>(),
            events.Select(e => e.EventId).ToArray()));
    }
}
