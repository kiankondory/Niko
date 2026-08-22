// ============================================================================
// Niko.Core — UserSettingsValidation.cs
// ----------------------------------------------------------------------------
// مسئولیت: اعتبارسنجی خالص تنظیمات کاربر (مصرف روزانه، قیمت، اندازهٔ بسته، ارز و
//           تاریخ ترک) برای ذخیرهٔ امن. مقادیر صفر/منفی/ناقص/تاریخ آینده رد می‌شوند.
// وابستگی‌ها و لایه: بخش Domain/Settings در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: فقط منطق اعتبارسنجی؛ پیام‌های کاربر در منابع locale هستند.
// ============================================================================

namespace Niko.Core.Domain.Settings;

using Niko.Core.Domain.Localization;

/// <summary>
/// نتیجهٔ اعتبارسنجی تنظیمات کاربر.
/// </summary>
public enum UserSettingsValidationResult
{
    /// <summary>همهٔ مقادیر معتبر.</summary>
    Valid = 0,

    /// <summary>مصرف روزانه صفر یا منفی است.</summary>
    InvalidCigarettesPerDay = 1,

    /// <summary>قیمت (هر نخ/هر بسته) صفر یا منفی است.</summary>
    InvalidPrice = 2,

    /// <summary>اندازهٔ بسته صفر یا منفی است.</summary>
    InvalidPackSize = 3,

    /// <summary>هیچ منبع قیمتی معتبری (هر نخ یا بسته) وجود ندارد.</summary>
    MissingPrice = 4,

    /// <summary>تاریخ ترک در آینده است.</summary>
    InvalidQuitDate = 5,

    /// <summary>کد ارز خالی/نامعتبر است.</summary>
    InvalidCurrency = 6,

    /// <summary>نام نمایشی بیش از حد طولانی است.</summary>
    InvalidDisplayName = 7,

    /// <summary>شناسهٔ آواتار مجاز نیست.</summary>
    InvalidAvatar = 8,

    /// <summary>locale در فهرست پشتیبانی‌شده نیست.</summary>
    InvalidLocale = 9,
}

/// <summary>
/// اعتبارسنجی خالص پروفایل کاربر.
/// </summary>
public static class UserSettingsValidation
{
    /// <summary>
    /// بررسی پروفایل و برگرداندن اولین خطا یا Valid.
    /// </summary>
    public static UserSettingsValidationResult Validate(UserProfile profile, DateTimeOffset now)
    {
        if (profile.DisplayName?.Length > 80)
        {
            return UserSettingsValidationResult.InvalidDisplayName;
        }

        if (!AvatarOptions.IsSupported(profile.AvatarId))
        {
            return UserSettingsValidationResult.InvalidAvatar;
        }

        if (profile.PreferredLocale is not null && !SupportedLocales.IsConfigured(profile.PreferredLocale))
        {
            return UserSettingsValidationResult.InvalidLocale;
        }

        if (profile.CigarettesPerDay is not { } cpd || cpd <= 0)
        {
            return UserSettingsValidationResult.InvalidCigarettesPerDay;
        }

        if (profile.PricePerCigarette is not null && profile.PricePerCigarette <= 0)
        {
            return UserSettingsValidationResult.InvalidPrice;
        }

        if (profile.PricePerPack is not null && profile.PricePerPack <= 0)
        {
            return UserSettingsValidationResult.InvalidPrice;
        }

        if (profile.PackSize is not null && profile.PackSize <= 0)
        {
            return UserSettingsValidationResult.InvalidPackSize;
        }

        if (profile.EffectivePricePerCigarette is null)
        {
            return UserSettingsValidationResult.MissingPrice;
        }

        if (profile.QuitDateUtc is not { } quitDate)
        {
            return UserSettingsValidationResult.InvalidQuitDate;
        }

        if (quitDate > now)
        {
            return UserSettingsValidationResult.InvalidQuitDate;
        }

        if (string.IsNullOrWhiteSpace(profile.CurrencyCode))
        {
            return UserSettingsValidationResult.InvalidCurrency;
        }

        return UserSettingsValidationResult.Valid;
    }
}
