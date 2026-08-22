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
using Niko.Core.Localization;
using Niko.Core.UseCases.Dashboard;

namespace Niko.ViewModels;

public sealed record IslandMilestoneDisplay(string DaysText, string StatusText, string Icon);

public sealed class IslandViewModel : INotifyPropertyChanged
{
    private readonly DashboardUseCase _dashboard;
    private readonly ILocalizationService _localization;
    private bool _isLoading = true;
    private bool _hasData;
    private string _streakText = string.Empty;
    private string _nextMilestoneText = string.Empty;
    private double _progress;

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
    public bool IsLoading { get => _isLoading; private set => Set(ref _isLoading, value); }
    public bool HasData { get => _hasData; private set => Set(ref _hasData, value); }
    public bool IsEmpty => !HasData && !IsLoading;
    public string StreakText { get => _streakText; private set => Set(ref _streakText, value); }
    public string NextMilestoneText { get => _nextMilestoneText; private set => Set(ref _nextMilestoneText, value); }
    public double Progress { get => _progress; private set => Set(ref _progress, value); }
    public ObservableCollection<IslandMilestoneDisplay> Milestones { get; } = new();
    public Command RefreshCommand { get; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var snapshot = (await _dashboard.ExecuteAsync()).Snapshot;
            var culture = CultureInfo.GetCultureInfo(_localization.CurrentLocale);
            HasData = snapshot.TotalSmoked + snapshot.TotalResisted + snapshot.TotalCravings > 0;
            StreakText = snapshot.CurrentStreakDays.ToString(culture);
            NextMilestoneText = snapshot.NextMilestoneDays.ToString(culture);
            Progress = Math.Clamp(snapshot.MilestoneProgressPercent / 100d, 0d, 1d);
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

    private void Set<T>(ref T field, T value, [System.Runtime.CompilerServices.CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnChanged(name);
    }

    private void OnChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
