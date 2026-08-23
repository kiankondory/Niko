// ============================================================================
// Niko.App — IDeviceConfirmationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد تأیید هویت محلی پیش از عملیات مخرب داده.
// وابستگی‌ها و لایه: MAUI presentation → adapter پلتفرم؛ هیچ داده‌ای ذخیره یا ارسال نمی‌کند.
// نکات تغییر و قیود: فقط قفل خود دستگاه پذیرفته می‌شود و نبود آن باید fail-closed باشد.
// ============================================================================

namespace Niko.Services;

public interface IDeviceConfirmationService
{
    /// <summary>
    /// درخواست تأیید با قفل واقعی دستگاه با عنوان و توضیح محلیِ UI.
    /// adapter پلتفرم نباید متن ثابت یا راز کاربر را تولید/ذخیره کند.
    /// </summary>
    Task<bool> ConfirmSensitiveActionAsync(
        string title,
        string description,
        CancellationToken ct = default);
}
