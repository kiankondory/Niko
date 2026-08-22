// ============================================================================
// Niko.Core — SaveUserSettingsResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ ذخیرهٔ تنظیمات کاربر؛ مشخص می‌کند ذخیره موفق بود یا خطای
//           اعتبارسنجی رخ داد.
// وابستگی‌ها و لایه: بخش UseCases/Settings در Core.
// نکات تغییر و قیود: فقط دادهٔ نتیجه؛ هیچ منطقی ندارد.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.Settings;

namespace Niko.Core.UseCases.Settings;

/// <summary>
/// نتیجهٔ ذخیرهٔ تنظیمات کاربر.
/// </summary>
public sealed record SaveUserSettingsResult(
    bool IsValid,
    UserSettingsValidationResult? Error = null,
    UserProfile? SavedProfile = null);
