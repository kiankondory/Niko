// ============================================================================
// Niko.App (Android) — AndroidNotificationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: آداپتر پلتفرمی INotificationService برای اندروید. مجوز POST_NOTIFICATIONS
//           را با MAUI Permissions فقط در نقطهٔ opt-in درخواست می‌کند و اعلان‌ها را
//           با Plugin.LocalNotification (Droid) محلی و آفلاین برنامه‌ریزی می‌کند.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI، فقط اندروید) → Core.Abstractions +
//           ILocalizationService. فقط در پلتفرم Android کامپایل می‌شود.
// نکات تغییر و قیود: پیش‌نمایش حاوی دادهٔ حساس نیست (قابل تنظیم برای عدم نمایش روی
//           صفحهٔ قفل). وضعیت رد مجوز به‌صورت امن مدیریت می‌شود.
// ============================================================================

using Android.App;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Platform.Droid;

namespace Niko.Services;

/// <summary>
/// سرویس اعلان محلی اندروید.
/// </summary>
public sealed class AndroidNotificationService : INotificationService
{
    private readonly ILocalizationService _localization;
    private readonly LocalNotificationService _service = new();

    public AndroidNotificationService(ILocalizationService localization)
    {
        _localization = localization;
    }

    public async Task<bool> IsPermissionGrantedAsync(CancellationToken ct = default)
    {
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted;
    }

    public async Task<bool> RequestPermissionAsync(CancellationToken ct = default)
    {
        var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted;
    }

    public Task ScheduleAsync(
        int id,
        NotificationContent content,
        DateTimeOffset fireAtUtc,
        CancellationToken ct = default)
    {
        var notification = new LocalNotification
        {
            NotificationId = id,
            Title = Resolve(content.TitleKey, content.TitleArgs),
            Description = Resolve(content.BodyKey, content.BodyArgs),
            NotifyTime = fireAtUtc.LocalDateTime,
        };

        _service.Show(notification);
        return Task.CompletedTask;
    }

    public Task CancelAsync(int id, CancellationToken ct = default)
    {
        _service.Cancel(id);
        return Task.CompletedTask;
    }

    public Task CancelAllAsync(CancellationToken ct = default)
    {
        _service.CancelAll();
        return Task.CompletedTask;
    }

    private string Resolve(string key, IReadOnlyDictionary<string, string>? args)
    {
        if (args is null || args.Count == 0)
        {
            return _localization.GetString(key);
        }

        var ordered = args.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToArray();
        return _localization.GetString(key, ordered);
    }
}
