// ============================================================================
// Niko.App — NoopNotificationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی موقت/بی‌عملی INotificationService برای پلتفرم‌هایی که هنوز
//           اعلان محلی ندارند (مانند Windows در این مرحله). در Android از
//           AndroidNotificationService استفاده می‌شود.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI) → Core.Abstractions.
// نکات تغییر و قیود: فقط placeholder است؛ نباید پیش‌فرض دائمی برای Android باشد.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;

namespace Niko.Services;

/// <summary>
/// سرویس اعلان بدون عملیات (برای پلتفرم‌های بدون اعلان محلی).
/// </summary>
public sealed class NoopNotificationService : INotificationService
{
    public Task<bool> IsPermissionGrantedAsync(CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<bool> RequestPermissionAsync(CancellationToken ct = default)
        => Task.FromResult(true);

    public Task ScheduleAsync(
        int id,
        NotificationContent content,
        DateTimeOffset fireAtUtc,
        CancellationToken ct = default)
        => Task.CompletedTask;

    public Task CancelAsync(int id, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task CancelAllAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
