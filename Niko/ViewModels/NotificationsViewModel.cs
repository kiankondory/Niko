// ============================================================================
// Niko.App — NotificationsViewModel.cs
// ----------------------------------------------------------------------------
// مسئولیت: مدل نمایش صفحهٔ اعلان‌ها. فقط مسئول ارائهٔ فرم و قالب‌بندی است؛ تمام
//           سیاست برنامه‌ریزی و مجوز در Core (NotificationSettingsUseCase) است.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI) → Core (NotificationSettingsUseCase,
//           ILocalizationService, Domain/Notifications).
// نکات تغییر و قیود: مجوز فقط هنگام فعال‌سازی درخواست می‌شود. هیچ متن خامی نداریم.
// ============================================================================

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Notifications;
using Niko.Core.Localization;
using Niko.Core.UseCases.Notifications;

namespace Niko.ViewModels;

/// <summary>
/// مدل نمایش صفحهٔ اعلان‌ها.
/// </summary>
public sealed class NotificationsViewModel : INotifyPropertyChanged
{
    private readonly NotificationSettingsUseCase _useCase;
    private readonly ILocalizationService _localization;

    private bool _dailyEnabled;
    private bool _milestoneEnabled;
    private bool _cravingEnabled;
    private bool _savingsEnabled;
    private TimeSpan _timeOfDay = new(9, 0, 0);
    private string _statusMessage = string.Empty;
    private bool _isEnabled;

    public NotificationsViewModel(
        NotificationSettingsUseCase useCase,
        ILocalizationService localization)
    {
        _useCase = useCase;
        _localization = localization;
        _localization.LocaleChanged += OnLocaleChanged;
        SaveCommand = new Command(async () => await SaveAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => _localization.GetString(LocalizationKeys.NotificationsTitle);

    public string EnableLabel => _localization.GetString(LocalizationKeys.NotificationsEnable);

    public string DailyLabel => _localization.GetString(LocalizationKeys.NotificationsDailyEncouragement);

    public string MilestoneLabel => _localization.GetString(LocalizationKeys.NotificationsMilestoneReached);

    public string CravingLabel => _localization.GetString(LocalizationKeys.NotificationsCravingSupport);

    public string SavingsLabel => _localization.GetString(LocalizationKeys.NotificationsSavingsProgress);

    public string TimeOfDayLabel => _localization.GetString(LocalizationKeys.NotificationsTimeOfDay);

    public string SaveLabel => _localization.GetString(LocalizationKeys.NotificationsSave);

    public string SensitiveHint => _localization.GetString(LocalizationKeys.NotificationsSensitiveHint);

    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetField(ref _isEnabled, value);
    }

    public bool DailyEnabled
    {
        get => _dailyEnabled;
        set => SetField(ref _dailyEnabled, value);
    }

    public bool MilestoneEnabled
    {
        get => _milestoneEnabled;
        set => SetField(ref _milestoneEnabled, value);
    }

    public bool CravingEnabled
    {
        get => _cravingEnabled;
        set => SetField(ref _cravingEnabled, value);
    }

    public bool SavingsEnabled
    {
        get => _savingsEnabled;
        set => SetField(ref _savingsEnabled, value);
    }

    public TimeSpan TimeOfDay
    {
        get => _timeOfDay;
        set => SetField(ref _timeOfDay, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public Command SaveCommand { get; }

    private void OnLocaleChanged(object? sender, EventArgs e)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

    /// <summary>بارگذاری ترجیحات ذخیره‌شده از Core (فقط ارائه).</summary>
    public async Task LoadAsync()
    {
        var prefs = await _useCase.LoadAsync();
        IsEnabled = prefs.IsAnythingEnabled;
        DailyEnabled = prefs.DailyEncouragementEnabled;
        MilestoneEnabled = prefs.MilestoneReachedEnabled;
        CravingEnabled = prefs.CravingSupportEnabled;
        SavingsEnabled = prefs.SavingsProgressEnabled;
        if (prefs.TimeOfDay is { } t)
        {
            TimeOfDay = t.ToTimeSpan();
        }

        StatusMessage = string.Empty;
    }

    private async Task SaveAsync()
    {
        // اگر هیچ دسته‌ای فعال نباشد، اعلان‌ها خاموش می‌شوند.
        var enabledCategories = BuildEnabledCategories();
        var prefs = new NotificationPreferences
        {
            DailyEncouragementEnabled = enabledCategories.Contains(NotificationCategory.DailyEncouragement),
            MilestoneReachedEnabled = enabledCategories.Contains(NotificationCategory.MilestoneReached),
            CravingSupportEnabled = enabledCategories.Contains(NotificationCategory.CravingSupport),
            SavingsProgressEnabled = enabledCategories.Contains(NotificationCategory.SavingsProgress),
            TimeOfDay = TimeOnly.FromTimeSpan(TimeOfDay),
        };

        var result = await _useCase.SaveAsync(prefs);

        StatusMessage = result.PermissionDenied
            ? _localization.GetString(LocalizationKeys.NotificationsPermissionDenied)
            : _localization.GetString(LocalizationKeys.NotificationsSaved);
    }

    private List<NotificationCategory> BuildEnabledCategories()
    {
        var list = new List<NotificationCategory>();
        if (DailyEnabled) list.Add(NotificationCategory.DailyEncouragement);
        if (MilestoneEnabled) list.Add(NotificationCategory.MilestoneReached);
        if (CravingEnabled) list.Add(NotificationCategory.CravingSupport);
        if (SavingsEnabled) list.Add(NotificationCategory.SavingsProgress);
        return list;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}
