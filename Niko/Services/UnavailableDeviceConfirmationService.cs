// ============================================================================
// Niko.App — UnavailableDeviceConfirmationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: پاسخ امن برای پلتفرم‌های بدون تأیید قفل دستگاه.
// وابستگی‌ها و لایه: MAUI presentation → IDeviceConfirmationService؛ بدون Core یا SQLite.
// نکات تغییر و قیود: پاک‌سازی داده در پلتفرم ناشناخته هرگز مجاز نمی‌شود.
// ============================================================================

namespace Niko.Services;

public sealed class UnavailableDeviceConfirmationService : IDeviceConfirmationService
{
    public Task<bool> ConfirmSensitiveActionAsync(
        string title,
        string description,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(false);
    }
}
