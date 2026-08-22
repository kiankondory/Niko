// ============================================================================
// Niko.Core — NotificationSettingsResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ به‌روزرسانی تنظیمات اعلان؛ شامل وضعیت مجوز و ترجیحات ذخیره‌شده.
// وابستگی‌ها و لایه: بخش UseCases/Notifications در Core.
// نکات تغییر و قیود: فقط دادهٔ نتیجه؛ هیچ منطقی ندارد.
// ============================================================================

using Niko.Core.Domain.Notifications;

namespace Niko.Core.UseCases.Notifications;

/// <summary>
/// نتیجهٔ ذخیرهٔ تنظیمات اعلان.
/// </summary>
public sealed record NotificationSettingsResult(
    NotificationPreferences Preferences,
    bool PermissionGranted,
    bool PermissionDenied);
