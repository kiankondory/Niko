// ============================================================================
// Niko.Core — INotificationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد سرویس اعلان محلی. هسته فقط این قرارداد را می‌شناسد؛
//           پیاده‌سازی پلتفرمی (Android/iOS) در لایهٔ App قرار دارد.
// وابستگی‌ها و لایه: بخش Abstractions در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: مجوز فقط هنگام فعال‌سازی (opt-in) درخواست می‌شود. برنامه‌ریزی
//           محلی و آفلاین است و به سرور نیاز ندارد. پیش‌نمایش حاوی دادهٔ حساس نیست.
// ============================================================================

using Niko.Core.Domain.Notifications;

namespace Niko.Core.Abstractions;

/// <summary>
/// قرارداد سرویس اعلان محلی.
/// </summary>
public interface INotificationService
{
    /// <summary>آیا مجوز اعلان اعطا شده است؟</summary>
    Task<bool> IsPermissionGrantedAsync(CancellationToken ct = default);

    /// <summary>درخواست مجوز اعلان (فقط در نقطهٔ opt-in).</summary>
    Task<bool> RequestPermissionAsync(CancellationToken ct = default);

    /// <summary>برنامه‌ریزی یک اعلان در زمان مشخص (UTC).</summary>
    Task ScheduleAsync(
        int id,
        NotificationContent content,
        DateTimeOffset fireAtUtc,
        CancellationToken ct = default);

    /// <summary>لغو یک اعلان.</summary>
    Task CancelAsync(int id, CancellationToken ct = default);

    /// <summary>لغو همهٔ اعلان‌های برنامه‌ریزی‌شده.</summary>
    Task CancelAllAsync(CancellationToken ct = default);
}
