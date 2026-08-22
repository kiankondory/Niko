// ============================================================================
// Niko.Core.Tests — InMemoryNotificationPreferencesStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی درون‌حافظهٔ INotificationPreferencesStore برای تست مورد
//           کاربرد تنظیمات اعلان.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای تست.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;

namespace Niko.Core.Tests;

/// <summary>
/// ذخیره‌ساز ترجیحات اعلان درون‌حافظهٔ تستی.
/// </summary>
public sealed class InMemoryNotificationPreferencesStore : INotificationPreferencesStore
{
    public NotificationPreferences? Preferences { get; set; }

    public Task<NotificationPreferences?> GetAsync(CancellationToken ct = default)
        => Task.FromResult(Preferences);

    public Task SaveAsync(NotificationPreferences preferences, CancellationToken ct = default)
    {
        Preferences = preferences;
        return Task.CompletedTask;
    }
}
