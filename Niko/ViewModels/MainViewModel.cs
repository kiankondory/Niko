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
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.QuickLog;
using Niko.Services;

namespace Niko.ViewModels;

/// <summary>
/// مدل نمایش صفحهٔ ثبت سریع.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly QuickLogUseCase _quickLog;
    private readonly DashboardUseCase _dashboard;
    private readonly ILocalizationService _localization;
    private readonly IWidgetRefreshService _widgetRefresh;
    private string _statusMessage = string.Empty;
    private int _smokedToday;
    private int _resistedToday;
    private int _cravingsToday;
    private bool _isStatusSuccess;

    public MainViewModel(
        QuickLogUseCase quickLog,
        DashboardUseCase dashboard,
        ILocalizationService localization,
        IWidgetRefreshService widgetRefresh)
    {
        _quickLog = quickLog;
        _dashboard = dashboard;
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

    public string Title => _localization.GetString(LocalizationKeys.QuickLogTitle);

    public string Subtitle => _localization.GetString(LocalizationKeys.QuickLogSubtitle);

    public string CravingBattleEntryLabel => _localization.GetString(LocalizationKeys.CravingBattleEntry);

    public string SmokedTodayText => string.Format(
        System.Globalization.CultureInfo.GetCultureInfo(_localization.CurrentLocale),
        _localization.GetString(LocalizationKeys.QuickLogSmokedToday),
        _smokedToday);

    public string ResistedTodayText => string.Format(
        System.Globalization.CultureInfo.GetCultureInfo(_localization.CurrentLocale),
        _localization.GetString(LocalizationKeys.QuickLogResistedToday),
        _resistedToday);

    public string CravingsTodayText => string.Format(
        System.Globalization.CultureInfo.GetCultureInfo(_localization.CurrentLocale),
        _localization.GetString(LocalizationKeys.QuickLogCravingsToday),
        _cravingsToday);

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
                OnPropertyChanged(nameof(IsStatusVisible));
            }
        }
    }

    public bool IsStatusVisible => !string.IsNullOrWhiteSpace(StatusMessage);

    public bool IsStatusSuccess
    {
        get => _isStatusSuccess;
        private set
        {
            if (_isStatusSuccess != value)
            {
                _isStatusSuccess = value;
                OnPropertyChanged();
            }
        }
    }

    public Command SmokedCommand { get; }

    public Command ResistedCommand { get; }

    public Command CravingCommand { get; }

    /// <summary>به‌روزرسانی نمایش شمارش‌های روز جاری از aggregate مشترک Core.</summary>
    public async Task LoadAsync()
    {
        var summary = (await _dashboard.ExecuteAsync()).DailySummary;
        _smokedToday = summary.SmokedToday;
        _resistedToday = summary.ResistedToday;
        _cravingsToday = summary.CravingsToday;
        OnPropertyChanged(nameof(SmokedTodayText));
        OnPropertyChanged(nameof(ResistedTodayText));
        OnPropertyChanged(nameof(CravingsTodayText));
    }

    private void OnLocaleChanged(object? sender, EventArgs e)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

    private async Task LogAsync(EventType type)
    {
        try
        {
            var result = await _quickLog.ExecuteAsync(new QuickLogRequest(type));
            await _widgetRefresh.RequestRefreshAsync();
            await LoadAsync();
            IsStatusSuccess = true;
            StatusMessage = type switch
            {
                EventType.Smoked => _localization.GetString(LocalizationKeys.QuickLogSuccessSmoked),
                EventType.Resisted => _localization.GetString(LocalizationKeys.QuickLogSuccessResisted),
                _ => _localization.GetString(LocalizationKeys.QuickLogSuccessCraving),
            };
        }
        catch
        {
            IsStatusSuccess = false;
            StatusMessage = _localization.GetString(LocalizationKeys.QuickLogError);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
