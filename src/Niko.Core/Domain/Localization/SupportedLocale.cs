// ============================================================================
// Niko.Core — SupportedLocale.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد یک locale قابل انتخاب، شامل نام بومی، جهت نوشتار و وضعیت
//           پوشش ترجمه. این مدل فقط دادهٔ پایدار UI را حمل می‌کند.
// وابستگی‌ها و لایه: Domain/Localization در Core؛ مستقل از MAUI و ResourceManager.
// نکات تغییر و قیود: نام‌ها کلید محلی‌سازی‌اند؛ IsFullyTranslated نباید بدون
//           وجود resource کامل true شود.
// ============================================================================

namespace Niko.Core.Domain.Localization;

public sealed record SupportedLocale(
    string Code,
    string NativeNameKey,
    bool IsRightToLeft,
    bool IsFullyTranslated);
