// ============================================================================
// Niko.App — MainViewModel.cs
// ----------------------------------------------------------------------------
// مسئولیت: ViewModel صفحهٔ اصلی (ثبت سریع). تنها رابط بین UI و مورد کاربرد ثبت
//           سریع است؛ هیچ محاسبهٔ دامنه‌ای در آن انجام نمی‌شود.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI) → Core (QuickLogUseCase, ILocalizationService).
// نکات تغییر و قیود: همهٔ متن‌ها از کلیدهای محلی‌سازی می‌آیند؛ هیچ متن خامی نداریم.
// ============================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Niko.Core.Abstractions;
using Niko.Core.Events;
using Niko.Core.Localization;
using Niko.Core.UseCases.QuickLog;
using Niko.Services;

namespace Niko.ViewModels;

/// <summary>
/// مدل نمایش صفحهٔ ثبت سریع.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly QuickLogUseCase _quickLog;
    private readonly ILocalizationService _localization;
    private readonly IWidgetRefreshService _widgetRefresh;
    private string _statusMessage = string.Empty;

    public MainViewModel(
        QuickLogUseCase quickLog,
        ILocalizationService localization,
        IWidgetRefreshService widgetRefresh)
    {
        _quickLog = quickLog;
        _localization = localization;
        _widgetRefresh = widgetRefresh;
        _localization.LocaleChanged += OnLocaleChanged;

        SmokedCommand = new Command(async () => await LogAsync(EventType.Smoked));
        ResistedCommand = new Command(async () => await LogAsync(EventType.Resisted));
        CravingCommand = new Command(async () => await LogAsync(EventType.Craving));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string SmokedLabel => _localization.GetString(LocalizationKeys.QuickLogSmoked);

    public string ResistedLabel => _localization.GetString(LocalizationKeys.QuickLogResisted);

    public string CravingLabel => _localization.GetString(LocalizationKeys.QuickLogCraving);

    public string CravingBattleEntryLabel => _localization.GetString(LocalizationKeys.CravingBattleEntry);

    public string SettingsEntryLabel => _localization.GetString(LocalizationKeys.SettingsEntry);

    public string ProfileEntryLabel => _localization.GetString(LocalizationKeys.ProfileEntry);

    public string NotificationsEntryLabel => _localization.GetString(LocalizationKeys.NotificationsEntry);

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public Command SmokedCommand { get; }

    public Command ResistedCommand { get; }

    public Command CravingCommand { get; }

    private void OnLocaleChanged(object? sender, EventArgs e)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

    private async Task LogAsync(EventType type)
    {
        try
        {
            var result = await _quickLog.ExecuteAsync(new QuickLogRequest(type));
            await _widgetRefresh.RequestRefreshAsync();
            StatusMessage = _localization.GetString(LocalizationKeys.QuickLogSuccess);
        }
        catch
        {
            StatusMessage = _localization.GetString(LocalizationKeys.QuickLogError);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
