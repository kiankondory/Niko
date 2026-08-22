// ============================================================================
// Niko.App — LocalizationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی آداپتر پلتفرمی ILocalizationService با ResourceManager مبتنی
//           بر resx. locale فعال را یافته، با fallback استاندارد .NET (دقیق → زبان →
//           خنثی) ترجمه می‌کند و کلید گمشده را گزارش می‌دهد.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI)؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: هیچ متن خام کاربر نمایش داده نمی‌شود؛ کلید گمشده فقط گزارش
//           و با placeholder امن جایگزین می‌شود. قالب‌بندی با CultureInfo فعال است.
// ============================================================================

using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using Niko.Core.Abstractions;

namespace Niko.Services;

/// <summary>
/// سرویس محلی‌سازی مبتنی بر منابع resx با fallback استاندارد و گزارش کلید گمشده.
/// </summary>
public sealed class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private readonly ILogger<LocalizationService> _logger;
    private CultureInfo _culture;
    private string _localeCode;

    public event EventHandler? LocaleChanged;

    public LocalizationService(ILogger<LocalizationService> logger)
    {
        _resourceManager = new ResourceManager(
            "Niko.Resources.Localization.Localization",
            typeof(App).Assembly);
        _logger = logger;
        _culture = CultureInfo.CurrentUICulture;
        _localeCode = string.IsNullOrWhiteSpace(_culture.Name) ? "en" : _culture.Name;
    }

    public string CurrentLocale => _localeCode;

    public void SetLocale(string locale)
    {
        var culture = string.IsNullOrWhiteSpace(locale)
            ? CultureInfo.GetCultureInfo("en")
            : CultureInfo.GetCultureInfo(locale);

        if (string.Equals(_localeCode, culture.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _culture = culture;
        _localeCode = culture.Name;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        LocaleChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetString(string key, params object[] args)
    {
        var value = _resourceManager.GetString(key, _culture);
        if (value is null)
        {
            _logger.LogWarning("کلید محلی‌سازی یافت نشد: {Key} (locale: {Locale})", key, _culture.Name);
            return $"{{missing:{key}}}";
        }

        return args.Length == 0
            ? value
            : string.Format(_culture, value, args);
    }
}
