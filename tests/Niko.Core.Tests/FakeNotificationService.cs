// ============================================================================
// Niko.Core.Tests — FakeNotificationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی شبیه‌سازی‌شدهٔ INotificationService برای تست تنظیمات اعلان.
//           امکان شبیه‌سازی رد/اعطای مجوز و ثبت اعلان‌های برنامه‌ریزی‌شده را می‌دهد.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای تست؛ بدون سرویس واقعی.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;

namespace Niko.Core.Tests;

/// <summary>
/// سرویس اعلان شبیه‌سازی‌شده.
/// </summary>
public sealed class FakeNotificationService : INotificationService
{
    public bool GrantPermission { get; set; } = true;
    public int PermissionRequests { get; private set; }
    public List<(int Id, NotificationContent Content, DateTimeOffset FireAt)> Scheduled { get; } = new();
    public int CancelAllCalls { get; private set; }
    public List<int> CancelledIds { get; } = new();

    public Task<bool> IsPermissionGrantedAsync(CancellationToken ct = default)
        => Task.FromResult(GrantPermission);

    public Task<bool> RequestPermissionAsync(CancellationToken ct = default)
    {
        PermissionRequests++;
        return Task.FromResult(GrantPermission);
    }

    public Task ScheduleAsync(
        int id,
        NotificationContent content,
        DateTimeOffset fireAtUtc,
        CancellationToken ct = default)
    {
        Scheduled.Add((id, content, fireAtUtc));
        return Task.CompletedTask;
    }

    public Task CancelAsync(int id, CancellationToken ct = default)
    {
        CancelledIds.Add(id);
        return Task.CompletedTask;
    }

    public Task CancelAllAsync(CancellationToken ct = default)
    {
        CancelAllCalls++;
        return Task.CompletedTask;
    }
}
