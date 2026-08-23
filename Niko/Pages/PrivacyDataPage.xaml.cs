// ============================================================================
// Niko.App — PrivacyDataPage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: اجرای export محلی و حذف محافظت‌شده با نمایش وضعیت قابل‌فهم.
// وابستگی‌ها و لایه: MAUI UI → PrivacyDataUseCase/IWidgetRefreshService/تأیید دستگاه.
// نکات تغییر و قیود: داده فقط در فایل محلی export می‌شود و حذف با قفل دستگاه fail-closed است.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Localization;
using Niko.Core.UseCases.Privacy;
using Niko.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace Niko.Pages;

public partial class PrivacyDataPage : ContentPage
{
    private readonly ILocalizationService _localization;
    private readonly PrivacyDataUseCase _privacy;
    private readonly IDeviceConfirmationService _deviceConfirmation;
    private readonly IWidgetRefreshService _widgetRefresh;
    private readonly ILogger<PrivacyDataPage> _logger;

    public PrivacyDataPage(
        ILocalizationService localization,
        PrivacyDataUseCase privacy,
        IDeviceConfirmationService deviceConfirmation,
        IWidgetRefreshService widgetRefresh,
        ILogger<PrivacyDataPage> logger)
    {
        InitializeComponent();
        _localization = localization;
        _privacy = privacy;
        _deviceConfirmation = deviceConfirmation;
        _widgetRefresh = widgetRefresh;
        _logger = logger;
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
        ExportTitleLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataExportTitle);
        ExportDescriptionLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataExportDescription);
        ExportButton.Text = _localization.GetString(LocalizationKeys.PrivacyDataExportAction);
        EraseTitleLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataEraseTitle);
        EraseDescriptionLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataEraseDescription);
        EraseButton.Text = _localization.GetString(LocalizationKeys.PrivacyDataEraseAction);
        Title = TitleLabel.Text;
    }

    private async void OnExportClicked(object? sender, EventArgs e)
    {
        SetBusy(true);
        try
        {
            var json = await _privacy.ExportJsonAsync();
            var directory = Path.Combine(FileSystem.CacheDirectory, "niko-exports");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"niko-export-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json");
            await File.WriteAllTextAsync(path, json);
            var request = new ShareFileRequest
            {
                Title = _localization.GetString(LocalizationKeys.PrivacyDataExportTitle),
                File = new ShareFile(path, "application/json"),
            };
            // Share sheet روی Android باید از thread رابط کاربری اجرا شود؛ در غیر این
            // صورت ممکن است هیچ sheetی دیده نشود، در حالی که export محلی درست ساخته شده است.
            await MainThread.InvokeOnMainThreadAsync(() => Share.Default.RequestAsync(request));
            StatusLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataExported);
        }
        catch (Exception exception)
        {
            _logger.LogWarning("آماده‌سازی export داده ناموفق بود. نوع خطا: {ExceptionType}", exception.GetType().Name);
            StatusLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataExportError);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnEraseClicked(object? sender, EventArgs e)
    {
        var accepted = await DisplayAlertAsync(
            _localization.GetString(LocalizationKeys.PrivacyDataEraseConfirmTitle),
            _localization.GetString(LocalizationKeys.PrivacyDataEraseConfirmBody),
            _localization.GetString(LocalizationKeys.PrivacyDataEraseConfirmAction),
            _localization.GetString(LocalizationKeys.PrivacyDataCancel));
        if (!accepted)
        {
            return;
        }

        if (!await _deviceConfirmation.ConfirmSensitiveActionAsync(
                _localization.GetString(LocalizationKeys.PrivacyDataEraseConfirmTitle),
                _localization.GetString(LocalizationKeys.PrivacyDataEraseConfirmBody)))
        {
            StatusLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataDeviceConfirmationRequired);
            return;
        }

        var erased = false;
        SetBusy(true);
        try
        {
            await _privacy.EraseAllAsync();
            await _widgetRefresh.RequestRefreshAsync();
            StatusLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataErased);
            erased = true;
        }
        catch (Exception exception)
        {
            _logger.LogWarning("پاک‌سازی دادهٔ محلی ناموفق بود. نوع خطا: {ExceptionType}", exception.GetType().Name);
            StatusLabel.Text = _localization.GetString(LocalizationKeys.PrivacyDataEraseError);
        }
        finally
        {
            SetBusy(false);
        }

        if (erased && Application.Current is global::Niko.App app)
        {
            app.RestartOnboardingAfterDataErasure();
        }
    }

    private void SetBusy(bool isBusy)
    {
        ExportButton.IsEnabled = !isBusy;
        EraseButton.IsEnabled = !isBusy;
        BusyIndicator.IsVisible = isBusy;
        BusyIndicator.IsRunning = isBusy;
    }
}
