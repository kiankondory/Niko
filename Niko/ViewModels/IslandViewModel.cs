// ============================================================================
// Niko.App — IslandViewModel.cs
// ----------------------------------------------------------------------------
// مسئولیت: آماده‌سازی نمایش تصویری Island از snapshot واقعی پیشرفت کاربر.
// وابستگی‌ها و لایه: MAUI presentation → DashboardUseCase و ILocalizationService در Core.
// نکات تغییر و قیود: هیچ XP، level یا دادهٔ ساختگی تولید نمی‌کند؛ فقط دادهٔ آفلاین
// و مشتق‌شدهٔ Core را نمایش می‌دهد و متن‌ها کاملاً محلی‌سازی‌شده‌اند.
// ============================================================================

using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using Niko.Core.Abstractions;
using Niko.Core.Domain;
using Niko.Core.Domain.Recovery;
using Niko.Core.Domain.Island;
using Niko.Core.Localization;
using Niko.Core.UseCases.Dashboard;

namespace Niko.ViewModels;

public sealed record IslandMilestoneDisplay(string DaysText, string StatusText, string Icon);
public sealed record IslandDailyReportDisplay(
    string DateText,
    string SmokedText,
    string ResistedText,
    string SavedText);

public sealed class IslandViewModel : INotifyPropertyChanged
{
    private readonly DashboardUseCase _dashboard;
    private readonly ILocalizationService _localization;
    private bool _isLoading = true;
    private bool _hasData;
    private string _streakText = string.Empty;
    private string _nextMilestoneText = string.Empty;
    private string _journeyStageText = string.Empty;
    private string _islandImageSource = "niko_island_stage_1.png";
    private double _progress;
    private string _cumulativeSavingsText = string.Empty;

    public IslandViewModel(DashboardUseCase dashboard, ILocalizationService localization)
    {
        _dashboard = dashboard;
        _localization = localization;
        RefreshCommand = new Command(async () => await LoadAsync());
        _localization.LocaleChanged += async (_, _) =>
        {
            OnChanged(null);
            await LoadAsync();
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => _localization.GetString(LocalizationKeys.IslandTitle);
    public string Subtitle => _localization.GetString(LocalizationKeys.IslandSubtitle);
    public string CurrentStreakLabel => _localization.GetString(LocalizationKeys.IslandCurrentStreak);
    public string NextMilestoneLabel => _localization.GetString(LocalizationKeys.IslandNextMilestone);
    public string ProgressLabel => _localization.GetString(LocalizationKeys.IslandProgress);
    public string EmptyText => _localization.GetString(LocalizationKeys.IslandEmpty);
    public string JourneyTitle => _localization.GetString(LocalizationKeys.IslandJourneyTitle);
    public string JourneyUnavailableText => _localization.GetString(LocalizationKeys.IslandJourneyUnavailable);
    public string DailyReportsTitle => _localization.GetString(LocalizationKeys.IslandDailyReportsTitle);
    public string ReportDateLabel => _localization.GetString(LocalizationKeys.IslandReportDate);
    public string ReportSmokedLabel => _localization.GetString(LocalizationKeys.IslandReportSmoked);
    public string ReportResistedLabel => _localization.GetString(LocalizationKeys.IslandReportResisted);
    public string ReportSavedLabel => _localization.GetString(LocalizationKeys.IslandReportSaved);
    public string CumulativeSavingsLabel => _localization.GetString(LocalizationKeys.IslandCumulativeSavings);
    public string SavingsUnavailableText => _localization.GetString(LocalizationKeys.IslandSavingsUnavailable);
    public bool IsLoading { get => _isLoading; private set => Set(ref _isLoading, value); }
    public bool HasData { get => _hasData; private set => Set(ref _hasData, value); }
    public bool IsEmpty => !HasData && !IsLoading;
    public string StreakText { get => _streakText; private set => Set(ref _streakText, value); }
    public string NextMilestoneText { get => _nextMilestoneText; private set => Set(ref _nextMilestoneText, value); }
    public double Progress { get => _progress; private set => Set(ref _progress, value); }
    public string JourneyStageText { get => _journeyStageText; private set => Set(ref _journeyStageText, value); }
    public string IslandImageSource { get => _islandImageSource; private set => Set(ref _islandImageSource, value); }
    public string CumulativeSavingsText { get => _cumulativeSavingsText; private set => Set(ref _cumulativeSavingsText, value); }
    public ObservableCollection<IslandMilestoneDisplay> Milestones { get; } = new();
    public ObservableCollection<IslandDailyReportDisplay> DailyReports { get; } = new();
    public Command RefreshCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var dashboard = await _dashboard.ExecuteAsync();
            var snapshot = dashboard.Snapshot;
            var culture = CultureInfo.GetCultureInfo(_localization.CurrentLocale);
            HasData = snapshot.TotalSmoked + snapshot.TotalResisted + snapshot.TotalCravings > 0;
            StreakText = snapshot.CurrentStreakDays.ToString(culture);
            NextMilestoneText = snapshot.NextMilestoneDays.ToString(culture);
            Progress = Math.Clamp(snapshot.MilestoneProgressPercent / 100d, 0d, 1d);
            var journey = RecoveryJourneyCalculator.Calculate(snapshot.Recovery);
            IslandImageSource = ImageSourceFor(journey.Stage);
            JourneyStageText = journey.IsAvailable
                ? _localization.GetString(JourneyStageKey(journey.Stage))
                : JourneyUnavailableText;
            CumulativeSavingsText = FormatMoney(dashboard.IslandCumulativeSavings, dashboard.DailySavingsCurrencyCode, culture);
            DailyReports.Clear();
            // تاریخچه در Core کامل می‌ماند؛ Island فقط پنجرهٔ فشردهٔ ۱۵ روز اخیر را نمایش می‌دهد.
            var reports = dashboard.IslandDailyReports.AsEnumerable().Reverse().Take(15).ToArray();
            foreach (var report in reports)
            {
                DailyReports.Add(new IslandDailyReportDisplay(
                    report.Date.ToString("d", culture),
                    string.Format(culture, _localization.GetString(LocalizationKeys.IslandSmokedValue), report.SmokedCount),
                    string.Format(culture, _localization.GetString(LocalizationKeys.IslandResistedValue), report.ResistedCount),
                    report.SavedAmount is { } saved
                        ? string.Format(culture, _localization.GetString(LocalizationKeys.IslandDailySavedValue), FormatMoney(saved, dashboard.DailySavingsCurrencyCode, culture))
                        : SavingsUnavailableText));
            }
            Milestones.Clear();
            foreach (var milestone in snapshot.Milestones)
            {
                var statusKey = milestone.Status switch
                {
                    MilestoneStatus.Completed => LocalizationKeys.MilestoneCompleted,
                    MilestoneStatus.Current => LocalizationKeys.MilestoneCurrent,
                    _ => LocalizationKeys.MilestoneUpcoming,
                };
                var icon = milestone.Status switch
                {
                    MilestoneStatus.Completed => "✓",
                    MilestoneStatus.Current => "★",
                    _ => "🔒",
                };
                Milestones.Add(new IslandMilestoneDisplay(
                    string.Format(culture, _localization.GetString(LocalizationKeys.MilestoneDays), milestone.ThresholdDays),
                    _localization.GetString(statusKey),
                    icon));
            }
        }
        finally
        {
            IsLoading = false;
            OnChanged(nameof(IsEmpty));
        }
    }

    private string FormatMoney(decimal? amount, string? currencyCode, CultureInfo culture) =>
        amount is { } value && !string.IsNullOrWhiteSpace(currencyCode)
            ? $"{value.ToString("N2", culture)} {currencyCode}"
            : SavingsUnavailableText;

    private static string ImageSourceFor(RecoveryJourneyStage stage) => stage switch
    {
        RecoveryJourneyStage.Garden => "niko_island_stage_2.png",
        RecoveryJourneyStage.Forest => "niko_island_stage_3.png",
        RecoveryJourneyStage.Haven => "niko_island_stage_4.png",
        _ => "niko_island_stage_1.png",
    };

    private static string JourneyStageKey(RecoveryJourneyStage stage) => stage switch
    {
        RecoveryJourneyStage.Garden => LocalizationKeys.IslandStageGarden,
        RecoveryJourneyStage.Forest => LocalizationKeys.IslandStageForest,
        RecoveryJourneyStage.Haven => LocalizationKeys.IslandStageHaven,
        _ => LocalizationKeys.IslandStageSeedling,
    };

    private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnChanged(name);
    }

    private void OnChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
