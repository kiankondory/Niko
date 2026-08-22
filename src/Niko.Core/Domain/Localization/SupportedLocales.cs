// ============================================================================
// Niko.Core — SupportedLocales.cs
// ----------------------------------------------------------------------------
// مسئولیت: فهرست یکتای localeهای انتخاب‌پذیر و تشخیص تنظیم معتبر. این فهرست
//           منبع مشترک Core و لایهٔ ارائه برای fallback امن است.
// وابستگی‌ها و لایه: Domain/Localization در Core؛ بدون وابستگی به فایل یا پلتفرم.
// نکات تغییر و قیود: فقط en، fa، ar و zh-Hans در این نسخه resource کامل دارند؛
//           سایر localeها با fallback خنثی English نمایش داده می‌شوند.
// ============================================================================

namespace Niko.Core.Domain.Localization;

public static class SupportedLocales
{
    public static IReadOnlyList<SupportedLocale> All { get; } =
        new[]
        {
            new SupportedLocale("en", "Language.English", false, true),
            new SupportedLocale("fa", "Language.Persian", true, true),
            new SupportedLocale("ar", "Language.Arabic", true, true),
            new SupportedLocale("de", "Language.German", false, false),
            new SupportedLocale("es", "Language.Spanish", false, false),
            new SupportedLocale("fr", "Language.French", false, false),
            new SupportedLocale("hi", "Language.Hindi", false, false),
            new SupportedLocale("id", "Language.Indonesian", false, false),
            new SupportedLocale("ja", "Language.Japanese", false, false),
            new SupportedLocale("ko", "Language.Korean", false, false),
            new SupportedLocale("pt-BR", "Language.PortugueseBrazil", false, false),
            new SupportedLocale("ru", "Language.Russian", false, false),
            new SupportedLocale("tr", "Language.Turkish", false, false),
            new SupportedLocale("uk", "Language.Ukrainian", false, false),
            new SupportedLocale("zh-Hans", "Language.ChineseSimplified", false, true),
            new SupportedLocale("zh-Hant", "Language.ChineseTraditional", false, false),
        };

    public static bool IsConfigured(string? locale)
        => locale is not null && All.Any(option =>
            string.Equals(option.Code, locale, StringComparison.OrdinalIgnoreCase));

    public static SupportedLocale Get(string locale)
        => All.First(option => string.Equals(option.Code, locale, StringComparison.OrdinalIgnoreCase));
}
