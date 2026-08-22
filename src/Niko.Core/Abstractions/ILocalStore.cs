// ============================================================================
// Niko.Core — ILocalStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد ذخیره‌سازی محلی رویدادها و پروفایل. هسته فقط این قرارداد را
//           می‌شناسد؛ پیاده‌سازی (SQLite) در لایهٔ Infrastructure قرار دارد.
// وابستگی‌ها و لایه: بخش Abstractions در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: رویدادها ابتدا به‌صورت محلی و پایدار ذخیره می‌شوند
//           (offline-first). عملیات باید اتمیک باشند و از idempotency پشتیبانی کنند.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Abstractions;

/// <summary>
/// قرارداد ذخیره‌سازی محلی رویدادها و پروفایل کاربر.
/// </summary>
public interface ILocalStore
{
    /// <summary>ذخیرهٔ یک رویداد. اگر شناسه تکراری باشد، عملیات idempotent است.</summary>
    Task SaveEventAsync(LogEvent logEvent, CancellationToken ct = default);

    /// <summary>بازیابی رویدادها به‌صورت صفحه‌بندی‌شده و مرتب بر پایهٔ زمان.</summary>
    Task<IReadOnlyList<LogEvent>> GetEventsAsync(
        int offset = 0,
        int limit = 100,
        CancellationToken ct = default);

    /// <summary>بازیابی رویدادهای در انتظار همگام‌سازی.</summary>
    Task<IReadOnlyList<LogEvent>> GetPendingEventsAsync(
        int limit = 100,
        CancellationToken ct = default);

    /// <summary>به‌روزرسانی وضعیت همگام‌سازی یک رویداد.</summary>
    Task UpdateSyncStatusAsync(
        string eventId,
        SyncStatus status,
        CancellationToken ct = default);
}
