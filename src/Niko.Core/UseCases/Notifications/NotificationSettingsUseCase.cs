// ============================================================================
// Niko.Core — NotificationSettingsUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: مورد کاربرد تنظیمات اعلان. ترجیحات را از ذخیره‌ساز محلی بارگذاری/ذخیره
//           می‌کند، در نقطهٔ opt-in مجوز را درخواست می‌کند و سپس اعلان‌های فعال را
//           برنامه‌ریزی یا اعلان‌های غیرفعال را لغو می‌کند. آفلاین و بدون سرور است.
// وابستگی‌ها و لایه: UseCases/Notifications → Abstractions (INotificationPreferencesStore,
//           INotificationService, IClock) + Domain/Notifications.
// نکات تغییر و قیود: مجوز فقط هنگام فعال‌سازی درخواست می‌شود. اگر مجوز رد شود،
//           ترجیحات ذخیره می‌شوند اما اعلان‌ها برنامه‌ریزی نمی‌شوند و نتیجهٔ رد گزارش می‌شود.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;

namespace Niko.Core.UseCases.Notifications;

/// <summary>
/// مورد کاربرد تنظیمات اعلان محلی.
/// </summary>
public sealed class NotificationSettingsUseCase
{
    private readonly INotificationPreferencesStore _store;
    private readonly INotificationService _service;
    private readonly IClock _clock;

    public NotificationSettingsUseCase(
        INotificationPreferencesStore store,
        INotificationService service,
        IClock clock)
    {
        _store = store;
        _service = service;
        _clock = clock;
    }

    /// <summary>بارگذاری ترجیحات؛ اگر ذخیره نشده باشد پیش‌فرض امن (غیرفعال).</summary>
    public async Task<NotificationPreferences> LoadAsync(CancellationToken ct = default)
    {
        return await _store.GetAsync(ct).ConfigureAwait(false) ?? new NotificationPreferences();
    }

    /// <summary>
    /// ذخیرهٔ ترجیحات، درخواست مجوز در صورت فعال‌سازی (opt-in) و اعمال برنامه‌ریزی.
    /// </summary>
    public async Task<NotificationSettingsResult> SaveAsync(
        NotificationPreferences preferences,
        CancellationToken ct = default)
    {
        // در صورت فعال بودن هر دسته، مجوز را در نقطهٔ opt-in درخواست می‌کنیم.
        var permissionGranted = true;
        if (preferences.IsAnythingEnabled)
        {
            permissionGranted = await _service.RequestPermissionAsync(ct).ConfigureAwait(false);
        }

        await _store.SaveAsync(preferences, ct).ConfigureAwait(false);

        // اعمال برنامه‌ریزی: فقط دسته‌های فعال و فقط وقتی مجوز هست.
        if (preferences.IsAnythingEnabled && permissionGranted)
        {
            await ApplySchedulesAsync(preferences, ct).ConfigureAwait(false);
        }
        else
        {
            await _service.CancelAllAsync(ct).ConfigureAwait(false);
        }

        return new NotificationSettingsResult(
            preferences,
            PermissionGranted: permissionGranted,
            PermissionDenied: preferences.IsAnythingEnabled && !permissionGranted);
    }

    /// <summary>
    /// لغو همهٔ اعلان‌ها (وقتی همهٔ دسته‌ها غیرفعال شوند).
    /// </summary>
    public async Task CancelAllAsync(CancellationToken ct = default)
    {
        await _service.CancelAllAsync(ct).ConfigureAwait(false);
    }

    private async Task ApplySchedulesAsync(
        NotificationPreferences preferences,
        CancellationToken ct)
    {
        // ابتدا همه را لغو می‌کنیم تا بدون تکرار برنامه‌ریزی کنیم.
        await _service.CancelAllAsync(ct).ConfigureAwait(false);

        foreach (NotificationCategory category in Enum.GetValues<NotificationCategory>())
        {
            if (!preferences.IsEnabled(category))
            {
                continue;
            }

            var fireAt = NotificationSchedulePolicy.NextDailyOccurrence(_clock.UtcNow, preferences.TimeOfDay);
            if (fireAt is not { } time)
            {
                continue;
            }

            await _service.ScheduleAsync(
                NotificationSchedulePolicy.GetNotificationId(category),
                NotificationSchedulePolicy.GetContent(category),
                time,
                ct).ConfigureAwait(false);
        }
    }
}
