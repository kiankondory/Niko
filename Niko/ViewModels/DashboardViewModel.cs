// ============================================================================
// Niko.App — DashboardViewModel.cs
// ----------------------------------------------------------------------------
// مسئولیت: مدل نمایش داشبورد. فقط مسئول ارائهٔ دادهٔ مشتق‌شده از Core و قالب‌بندی
//           محلی است؛ هیچ محاسبهٔ دامنه‌ای در آن انجام نمی‌شود.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI) → Core (DashboardUseCase, ILocalizationService).
// نکات تغییر و قیود: همهٔ متن‌ها از کلیدهای محلی‌سازی می‌آیند. اعداد/درصد/ارز با
//           CultureInfo فعال قالب‌بندی می‌شوند.
// ============================================================================

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Niko.Core.Abstractions;
using Niko.Core.Domain;
using Niko.Core.Domain.Recovery;
using Niko.Core.Domain.TriggerAnalysis;
using Niko.Core.Localization;
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.TriggerAnalysis;

namespace Niko.ViewModels;

/// <summary>
/// نمای نمایشی یک میل‌استون برای ارائه در UI (بدون منطق دامنه).
/// </summary>
public sealed record MilestoneDisplay(
    string DaysText,
    string StatusText,
    bool IsCurrent,
    bool IsCompleted,
    Color TextColor,
    Color StrokeColor);

/// <summary>بینش تجمیعی آمادهٔ نمایش؛ مقدار خام زمینه هرگز در آن قرار نمی‌گیرد.</summary>
public sealed record TriggerInsightDisplay(string Text, string ApproximateText);

/// <summary>
/// مدل نمایش داشبورد.
/// </summary>
public sealed class DashboardViewModel : INotifyPropertyChanged
{
    private readonly DashboardUseCase _useCase;
    private readonly TriggerAnalysisUseCase _triggerAnalysisUseCase;
    private readonly ILocalizationService _localization;
    private readonly IFeatureFlagProvider _featureFlags;
    private readonly ILogger<DashboardViewModel> _logger;

    private string _smokedText = string.Empty;
    private string _resistedText = string.Empty;
    private string _cravingsText = string.Empty;
    private string _currentStreakText = string.Empty;
    private string _milestoneText = string.Empty;
    private double _milestoneProgress;
    private string _savingsText = string.Empty;
    private bool _isLoading = true;
    private bool _isEmpty;
    private bool _hasData;
    private string _recoveryStageTitle = string.Empty;
    private string _recoveryStageDescription = string.Empty;
    private string _recoveryNextStageText = string.Empty;
    private double _recoveryProgress;
    private bool _isRecoveryAvailable;
    private readonly ObservableCollection<MilestoneDisplay> _milestones = new();
    private readonly ObservableCollection<TriggerInsightDisplay> _triggerInsights = new();
    private bool _triggerAnalysisEnabled;
    private bool _triggerAnalysisHasSufficientData;
    private bool _triggerAnalysisError;

    public DashboardViewModel(
        DashboardUseCase useCase,
        TriggerAnalysisUseCase triggerAnalysisUseCase,
        ILocalizationService localization,
        IFeatureFlagProvider featureFlags,
        ILogger<DashboardViewModel> logger)
    {
        _useCase = useCase;
        _triggerAnalysisUseCase = triggerAnalysisUseCase;
        _localization = localization;
        _featureFlags = featureFlags;
        _logger = logger;
        _localization.LocaleChanged += OnLocaleChanged;
        RefreshCommand = new Command(async () => await LoadAsync());
        ToggleTriggerAnalysisCommand = new Command(async () => await ToggleTriggerAnalysisAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => _localization.GetString(LocalizationKeys.DashboardTitle);

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetField(ref _isLoading, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetField(ref _isEmpty, value);
    }

    public bool HasData
    {
        get => _hasData;
        private set => SetField(ref _hasData, value);
    }

    public string SmokedText
    {
        get => _smokedText;
        private set => SetField(ref _smokedText, value);
    }

    public string SmokedLabel => _localization.GetString(LocalizationKeys.DashboardSmoked);

    public string ResistedText
    {
        get => _resistedText;
        private set => SetField(ref _resistedText, value);
    }

    public string ResistedLabel => _localization.GetString(LocalizationKeys.DashboardResisted);

    public string CravingsText
    {
        get => _cravingsText;
        private set => SetField(ref _cravingsText, value);
    }

    public string CravingsLabel => _localization.GetString(LocalizationKeys.DashboardCravings);

    public string CurrentStreakText
    {
        get => _currentStreakText;
        private set => SetField(ref _currentStreakText, value);
    }

    public string MilestoneText
    {
        get => _milestoneText;
        private set => SetField(ref _milestoneText, value);
    }

    public double MilestoneProgress
    {
        get => _milestoneProgress;
        private set => SetField(ref _milestoneProgress, value);
    }

    public string SavingsText
    {
        get => _savingsText;
        private set => SetField(ref _savingsText, value);
    }

    public string DashboardEmptyText => _localization.GetString(LocalizationKeys.DashboardEmpty);

    public string DashboardGreeting => _localization.GetString(LocalizationKeys.DashboardGreeting);

    public string DashboardHeroTitle => _localization.GetString(LocalizationKeys.DashboardHeroTitle);

    public string DashboardHeroBody => _localization.GetString(LocalizationKeys.DashboardHeroBody);

    public string DashboardOverview => _localization.GetString(LocalizationKeys.DashboardOverview);

    public string DashboardActivity => _localization.GetString(LocalizationKeys.DashboardActivity);

    public string DashboardSavings => _localization.GetString(LocalizationKeys.DashboardSavings);

    public string DashboardCurrentStreak => _localization.GetString(LocalizationKeys.DashboardCurrentStreak);

    public string SavingsDisclaimer => _localization.GetString(LocalizationKeys.DashboardSavingsDisclaimer);

    public string MilestonesTitle => _localization.GetString(LocalizationKeys.DashboardMilestones);

    public IReadOnlyList<MilestoneDisplay> Milestones => _milestones;

    public string RecoveryTitle => _localization.GetString(LocalizationKeys.RecoveryTitle);

    public string RecoveryProgressLabel => _localization.GetString(LocalizationKeys.RecoveryProgress);

    public string RecoveryDisclaimer => _localization.GetString(LocalizationKeys.RecoveryDisclaimer);

    public string RecoveryUnavailableText => _localization.GetString(LocalizationKeys.RecoveryUnavailable);

    public string RecoveryStageTitle
    {
        get => _recoveryStageTitle;
        private set => SetField(ref _recoveryStageTitle, value);
    }

    public string RecoveryStageDescription
    {
        get => _recoveryStageDescription;
        private set => SetField(ref _recoveryStageDescription, value);
    }

    public string RecoveryNextStageText
    {
        get => _recoveryNextStageText;
        private set => SetField(ref _recoveryNextStageText, value);
    }

    public double RecoveryProgress
    {
        get => _recoveryProgress;
        private set => SetField(ref _recoveryProgress, value);
    }

    public bool IsRecoveryAvailable
    {
        get => _isRecoveryAvailable;
        private set
        {
            if (SetField(ref _isRecoveryAvailable, value))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRecoveryUnavailable)));
            }
        }
    }

    public bool IsRecoveryUnavailable => !_isRecoveryAvailable;

    public string RefreshText => _localization.GetString(LocalizationKeys.Refresh);

    public string TriggerAnalysisTitle => _localization.GetString(LocalizationKeys.TriggerAnalysisTitle);

    public string TriggerAnalysisToggleText => _localization.GetString(
        _triggerAnalysisEnabled ? LocalizationKeys.TriggerAnalysisDisable : LocalizationKeys.TriggerAnalysisEnable);

    public string TriggerAnalysisDisabledText => _localization.GetString(LocalizationKeys.TriggerAnalysisDisabled);

    public string TriggerAnalysisMinimumDataText => _localization.GetString(LocalizationKeys.TriggerAnalysisMinimumData);

    public string TriggerAnalysisInsufficientDataText => _localization.GetString(LocalizationKeys.TriggerAnalysisInsufficientData);

    public string TriggerAnalysisEmptyText => _localization.GetString(LocalizationKeys.TriggerAnalysisEmpty);

    public string TriggerAnalysisErrorText => _localization.GetString(LocalizationKeys.TriggerAnalysisError);

    public string TriggerAnalysisPrivacyNote => _localization.GetString(LocalizationKeys.TriggerAnalysisPrivacyNote);

    public string TriggerAnalysisDisclaimer => _localization.GetString(LocalizationKeys.TriggerAnalysisDisclaimer);

    public bool IsTriggerAnalysisEnabled => _triggerAnalysisEnabled;

    public bool IsTriggerAnalysisAvailable => _featureFlags.IsEnabled(FeatureFlag.TriggerAnalysisUi);

    public bool IsTriggerAnalysisDisabled => !_triggerAnalysisEnabled && !_triggerAnalysisError;

    public bool IsTriggerAnalysisInsufficientData =>
        _triggerAnalysisEnabled && !_triggerAnalysisError && !_triggerAnalysisHasSufficientData;

    public bool IsTriggerAnalysisEmpty =>
        _triggerAnalysisEnabled && !_triggerAnalysisError && _triggerAnalysisHasSufficientData && _triggerInsights.Count == 0;

    public bool IsTriggerAnalysisError => _triggerAnalysisError;

    public bool HasTriggerInsights => _triggerInsights.Count > 0;

    public IReadOnlyList<TriggerInsightDisplay> TriggerInsights => _triggerInsights;

    public Command RefreshCommand { get; }

    public Command ToggleTriggerAnalysisCommand { get; }

    private void OnLocaleChanged(object? sender, EventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        _ = MainThread.InvokeOnMainThreadAsync(LoadAsync);
    }

    /// <summary>بارگذاری دادهٔ داشبورد از Core (فقط ارائه).</summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _useCase.ExecuteAsync();
            Apply(result.Snapshot);
            if (IsTriggerAnalysisAvailable)
            {
                await LoadTriggerAnalysisAsync();
            }
            else
            {
                _triggerAnalysisEnabled = false;
                _triggerAnalysisHasSufficientData = false;
                _triggerAnalysisError = false;
                _triggerInsights.Clear();
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadTriggerAnalysisAsync()
    {
        try
        {
            var result = await _triggerAnalysisUseCase.AnalyzeAsync();
            _triggerAnalysisEnabled = result.IsEnabled;
            _triggerAnalysisHasSufficientData = result.HasSufficientData;
            _triggerAnalysisError = false;
            _triggerInsights.Clear();

            if (result.IsEnabled && result.HasSufficientData)
            {
                var culture = CultureInfo.GetCultureInfo(_localization.CurrentLocale);
                foreach (var insight in result.Insights)
                {
                    _triggerInsights.Add(ToDisplay(insight, culture));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "بارگذاری تحلیل محرک محلی ناموفق بود. نوع خطا: {ExceptionType}",
                ex.GetType().Name);
            _triggerAnalysisError = true;
            _triggerAnalysisEnabled = false;
            _triggerAnalysisHasSufficientData = false;
            _triggerInsights.Clear();
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }

    private async Task ToggleTriggerAnalysisAsync()
    {
        try
        {
            await _triggerAnalysisUseCase.SetEnabledAsync(!_triggerAnalysisEnabled);
            await LoadTriggerAnalysisAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "تغییر ترجیح تحلیل محرک محلی ناموفق بود. نوع خطا: {ExceptionType}",
                ex.GetType().Name);
            _triggerAnalysisError = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }

    private TriggerInsightDisplay ToDisplay(TriggerInsight insight, CultureInfo culture)
    {
        var text = insight.Kind switch
        {
            TriggerInsightKind.TimeOfDay => string.Format(
                culture,
                _localization.GetString(insight.LabelKey),
                _localization.GetString(Convert.ToString(insight.Args?["bucket"], CultureInfo.InvariantCulture) ?? string.Empty)),
            TriggerInsightKind.DayOfWeek => string.Format(
                culture,
                _localization.GetString(insight.LabelKey),
                culture.DateTimeFormat.GetDayName((DayOfWeek)Convert.ToInt32(insight.Args?["day"], CultureInfo.InvariantCulture))),
            TriggerInsightKind.Context => _localization.GetString(LocalizationKeys.TriggerInsightContextAggregated),
            TriggerInsightKind.CravingFrequency => string.Format(
                culture,
                _localization.GetString(insight.LabelKey),
                Convert.ToString(insight.Args?["count"], culture)),
            TriggerInsightKind.SmokedVsResisted => string.Format(
                culture,
                _localization.GetString(insight.LabelKey),
                Convert.ToInt32(insight.Args?["resisted"], CultureInfo.InvariantCulture),
                insight.Count),
            _ => string.Empty,
        };

        var approximate = string.Format(
            culture,
            _localization.GetString(LocalizationKeys.TriggerAnalysisStrength),
            _localization.GetString(LocalizationKeys.TriggerAnalysisApproximate),
            insight.Strength.ToString("0", culture));
        return new TriggerInsightDisplay(text, approximate);
    }

    private void Apply(ProgressSnapshot snapshot)
    {
        var culture = CultureInfo.GetCultureInfo(_localization.CurrentLocale);

        var anyEvents = snapshot.TotalSmoked + snapshot.TotalResisted + snapshot.TotalCravings > 0;
        IsEmpty = !anyEvents;
        HasData = anyEvents;

        SmokedText = snapshot.TotalSmoked.ToString(culture);
        ResistedText = snapshot.TotalResisted.ToString(culture);
        CravingsText = snapshot.TotalCravings.ToString(culture);

        CurrentStreakText = string.Format(
            culture,
            _localization.GetString(LocalizationKeys.DashboardCurrentStreak) + ": " +
            _localization.GetString(LocalizationKeys.DashboardDays),
            snapshot.CurrentStreakDays);

        MilestoneText = string.Format(
            culture,
            _localization.GetString(LocalizationKeys.DashboardMilestoneNext),
            snapshot.NextMilestoneDays);

        MilestoneProgress = Math.Clamp(snapshot.MilestoneProgressPercent / 100.0, 0.0, 1.0);

        PopulateMilestones(snapshot.Milestones, culture);

        PopulateRecovery(snapshot.Recovery, culture);

        SavingsText = snapshot.ApproximateSavings is { } savings
            ? _localization.GetString(LocalizationKeys.DashboardSavings) + ": " +
              FormatCurrency(culture, savings)
            : _localization.GetString(LocalizationKeys.DashboardSavingsUnavailable);
    }

    private void PopulateRecovery(RecoverySnapshot recovery, CultureInfo culture)
    {
        IsRecoveryAvailable = recovery.HasSufficientData;

        if (!recovery.HasSufficientData)
        {
            RecoveryStageTitle = string.Empty;
            RecoveryStageDescription = string.Empty;
            RecoveryNextStageText = string.Empty;
            RecoveryProgress = 0;
            return;
        }

        RecoveryStageTitle = _localization.GetString(StageTitleKey(recovery.Stage));
        RecoveryStageDescription = _localization.GetString(StageDescriptionKey(recovery.Stage));
        RecoveryProgress = Math.Clamp(recovery.ProgressPercent / 100.0, 0.0, 1.0);

        // برای آخرین مرحله، «مرحلهٔ بعدی» وجود ندارد.
        RecoveryNextStageText = recovery.Stage >= RecoveryStage.Stage7
            ? string.Empty
            : string.Format(
                culture,
                _localization.GetString(LocalizationKeys.RecoveryNextStage),
                _localization.GetString(StageTitleKey(recovery.Stage + 1)));
    }

    private static string StageTitleKey(RecoveryStage stage)
    {
        return stage switch
        {
            RecoveryStage.Stage0 => LocalizationKeys.RecoveryStage0Title,
            RecoveryStage.Stage1 => LocalizationKeys.RecoveryStage1Title,
            RecoveryStage.Stage2 => LocalizationKeys.RecoveryStage2Title,
            RecoveryStage.Stage3 => LocalizationKeys.RecoveryStage3Title,
            RecoveryStage.Stage4 => LocalizationKeys.RecoveryStage4Title,
            RecoveryStage.Stage5 => LocalizationKeys.RecoveryStage5Title,
            RecoveryStage.Stage6 => LocalizationKeys.RecoveryStage6Title,
            _ => LocalizationKeys.RecoveryStage7Title,
        };
    }

    private static string StageDescriptionKey(RecoveryStage stage)
    {
        return stage switch
        {
            RecoveryStage.Stage0 => LocalizationKeys.RecoveryStage0Description,
            RecoveryStage.Stage1 => LocalizationKeys.RecoveryStage1Description,
            RecoveryStage.Stage2 => LocalizationKeys.RecoveryStage2Description,
            RecoveryStage.Stage3 => LocalizationKeys.RecoveryStage3Description,
            RecoveryStage.Stage4 => LocalizationKeys.RecoveryStage4Description,
            RecoveryStage.Stage5 => LocalizationKeys.RecoveryStage5Description,
            RecoveryStage.Stage6 => LocalizationKeys.RecoveryStage6Description,
            _ => LocalizationKeys.RecoveryStage7Description,
        };
    }

    private void PopulateMilestones(
        IReadOnlyList<MilestoneInfo> milestones,
        CultureInfo culture)
    {
        _milestones.Clear();

        foreach (var milestone in milestones)
        {
            var isCurrent = milestone.Status == MilestoneStatus.Current;
            var isCompleted = milestone.Status == MilestoneStatus.Completed;
            var accent = isCurrent ? Colors.Orange : isCompleted ? Colors.Green : Colors.Gray;

            _milestones.Add(new MilestoneDisplay(
                DaysText: string.Format(
                    culture,
                    _localization.GetString(LocalizationKeys.MilestoneDays),
                    milestone.ThresholdDays),
                StatusText: StatusLabel(milestone.Status),
                IsCurrent: isCurrent,
                IsCompleted: isCompleted,
                TextColor: accent,
                StrokeColor: accent));
        }
    }

    private string StatusLabel(MilestoneStatus status)
    {
        return status switch
        {
            MilestoneStatus.Completed => _localization.GetString(LocalizationKeys.MilestoneCompleted),
            MilestoneStatus.Current => _localization.GetString(LocalizationKeys.MilestoneCurrent),
            _ => _localization.GetString(LocalizationKeys.MilestoneUpcoming),
        };
    }

    private static string FormatCurrency(CultureInfo culture, decimal amount)
    {
        return amount.ToString("C", culture);
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
