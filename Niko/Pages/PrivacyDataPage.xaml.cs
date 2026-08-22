// ============================================================================
// Niko.App — PrivacyDataPage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: نمایش entry point حریم خصوصی و کنترل داده به‌صورت محلی و شفاف.
// وابستگی‌ها و لایه: MAUI UI → ILocalizationService؛ هیچ داده‌ای را ارسال یا حذف نمی‌کند.
// نکات تغییر و قیود: عملیات export/delete در این گام عمداً پیاده‌سازی نشده‌اند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Localization;

namespace Niko.Pages;

public partial class PrivacyDataPage : ContentPage
{
    private readonly ILocalizationService _localization;

    public PrivacyDataPage(ILocalizationService localization)
    {
        InitializeComponent();
        _localization = localization;
        _localization.LocaleChanged += OnLocaleChanged;
        ApplyText();
    }

    private void OnLocaleChanged(object? sender, EventArgs e) => ApplyText();

    private void ApplyText()
    {
        TitleLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataTitle);
        DescriptionLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataDescription);
        LocalOnlyLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataLocalOnly);
        ControlsLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataControls);
        Title = TitleLabel.Text;
    }
}
