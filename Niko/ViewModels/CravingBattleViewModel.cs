// ============================================================================
// Niko.App — CravingBattleViewModel.cs
// ----------------------------------------------------------------------------
// مسئولیت: مدل نمایش نبرد با هوس. فقط مسئول ارائهٔ وضعیت و قالب‌بندی محلی است و
//           همهٔ قواعد وضعیت را از Core (CravingBattleUseCase) می‌گیرد. تایمر و
//           پیشرفت فقط برای نمایش است؛ منطق دامنه در Core است.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI) → Core (CravingBattleUseCase,
//           ILocalizationService, Domain/Craving).
// نکات تغییر و قیود: همهٔ متن‌ها از کلیدهای محلی‌سازی می‌آیند. خروج/مقاومت بدون
//           شرم است. تایمر درون‌حافظه است و با شروع دوباره بازنشانی می‌شود.
// ============================================================================

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Craving;
using Niko.Core.Domain.Coach;
using Niko.Core.Localization;
using Niko.Core.UseCases.Coach;
using Niko.Core.UseCases.CravingBattle;

namespace Niko.ViewModels;

/// <summary>
/// وضعیت نمایشی صفحهٔ نبرد با هوس.
/// </summary>
public enum CravingBattlePhase
{
    SelectIntensity,
    ChooseAction,
    Active,
    Completed,
    Resisted,
    Exited,
}

/// <summary>
/// مدل نمایش نبرد با هوس.
/// </summary>
public sealed class CravingBattleViewModel : INotifyPropertyChanged
{
    private readonly CravingBattleUseCase _useCase;
    private readonly ILocalizationService _localization;
    private readonly CoachUseCase _coachUseCase;
    private readonly ExternalCoachPrivacyGateway _externalCoachGateway;
    private CancellationTokenSource? _timerCts;
    private int _remainingSeconds;
    private int _totalSeconds;
    private CravingBattlePhase _phase = CravingBattlePhase.SelectIntensity;
    private Intervention? _currentIntervention;
    private string _statusMessage = string.Empty;

    public CravingBattleViewModel(
        CravingBattleUseCase useCase,
        ILocalizationService localization,
        CoachUseCase coachUseCase,
        ExternalCoachPrivacyGateway externalCoachGateway)
    {
        _useCase = useCase;
        _localization = localization;
        _coachUseCase = coachUseCase;
        _externalCoachGateway = externalCoachGateway;
        _localization.LocaleChanged += OnLocaleChanged;

        SelectIntensityCommand = new Command<CravingIntensity>(async i => await StartAsync(i));
        SelectActionCommand = new Command<Intervention>(async a => await SelectActionAsync(a));
        CompleteCommand = new Command(async () => await CompleteAsync());
        ResistCommand = new Command(async () => await ResistAsync());
        ExitCommand = new Command(async () => await ExitAsync());
        StartOverCommand = new Command(Reset);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => _localization.GetString(LocalizationKeys.CravingBattleTitle);

    public string SelectIntensityPrompt => _localization.GetString(LocalizationKeys.CravingBattleSelectIntensity);

    public string ChooseActionPrompt => _localization.GetString(LocalizationKeys.CravingBattleChooseAction);

    public string MildLabel => _localization.GetString(LocalizationKeys.CravingIntensityMild);

    public string ModerateLabel => _localization.GetString(LocalizationKeys.CravingIntensityModerate);

    public string IntenseLabel => _localization.GetString(LocalizationKeys.CravingIntensityIntense);

    public string CompleteLabel => _localization.GetString(LocalizationKeys.CravingBattleComplete);

    public string ResistLabel => _localization.GetString(LocalizationKeys.CravingBattleResistButton);

    public string ExitLabel => _localization.GetString(LocalizationKeys.CravingBattleExit);

    public string StartOverLabel => _localization.GetString(LocalizationKeys.CravingBattleStartOver);

    public CravingBattlePhase Phase
    {
        get => _phase;
        private set
        {
            if (_phase != value)
            {
                _phase = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIntensityPhase));
                OnPropertyChanged(nameof(IsActionPhase));
                OnPropertyChanged(nameof(IsActivePhase));
                OnPropertyChanged(nameof(IsCompletedPhase));
                OnPropertyChanged(nameof(IsResistedPhase));
                OnPropertyChanged(nameof(IsExitedPhase));
                OnPropertyChanged(nameof(IsResultPhase));
            }
        }
    }

    public bool IsIntensityPhase => Phase == CravingBattlePhase.SelectIntensity;

    public bool IsActionPhase => Phase == CravingBattlePhase.ChooseAction;

    public bool IsActivePhase => Phase == CravingBattlePhase.Active;

    public bool IsCompletedPhase => Phase == CravingBattlePhase.Completed;

    public bool IsResistedPhase => Phase == CravingBattlePhase.Resisted;

    public bool IsExitedPhase => Phase == CravingBattlePhase.Exited;

    public bool IsResultPhase => IsCompletedPhase || IsResistedPhase || IsExitedPhase;

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

    public string CoachSuggestionText { get; private set; } = string.Empty;

    public bool IsCoachSuggestionVisible => !string.IsNullOrWhiteSpace(CoachSuggestionText);

    public string TimerText { get; private set; } = string.Empty;

    public double TimerProgress { get; private set; }

    public string CurrentInterventionTitle { get; private set; } = string.Empty;

    public string CurrentInterventionGuide { get; private set; } = string.Empty;

    public string CurrentInterventionIconSource { get; private set; } = "battle_breathe.svg";

    public string IntensityLabel { get; private set; } = string.Empty;

    public string InterventionListTitle => _localization.GetString(LocalizationKeys.CravingBattleChooseAction);

    public ObservableCollection<InterventionDisplay> Interventions { get; } = new();

    public Command<CravingIntensity> SelectIntensityCommand { get; }

    public Command<Intervention> SelectActionCommand { get; }

    public Command CompleteCommand { get; }

    public Command ResistCommand { get; }

    public Command ExitCommand { get; }

    public Command StartOverCommand { get; }

    private void OnLocaleChanged(object? sender, EventArgs e)
    {
        IntensityLabel = Phase == CravingBattlePhase.SelectIntensity
            ? string.Empty
            : IntensityLabel;
        if (Interventions.Count > 0)
        {
            PopulateInterventions();
        }

        if (_currentIntervention is { } intervention)
        {
            CurrentInterventionTitle = ActionText(intervention);
            CurrentInterventionGuide = _localization.GetString(LocalizationKeys.CravingBattleInterventionGuide);
            UpdateTimerDisplay();
        }

        OnPropertyChanged(nameof(IntensityLabel));
        OnPropertyChanged(nameof(CurrentInterventionTitle));
        OnPropertyChanged(nameof(CurrentInterventionGuide));
        OnPropertyChanged(null);
    }

    private async Task StartAsync(CravingIntensity intensity)
    {
        CancelTimer();

        var result = await _useCase.StartAsync(intensity);
        IntensityLabel = IntensityText(result.Intensity);
        await LoadCoachSuggestionAsync((int)intensity);

        PopulateInterventions();

        Phase = CravingBattlePhase.ChooseAction;
        StatusMessage = string.Empty;
    }

    private async Task SelectActionAsync(Intervention intervention)
    {
        var result = await _useCase.SelectActionAsync(intervention);

        var display = Interventions.First(i => i.Intervention == intervention);
        _currentIntervention = intervention;
        CurrentInterventionTitle = display.Title;
        CurrentInterventionIconSource = display.IconSource;
        CurrentInterventionGuide = _localization.GetString(LocalizationKeys.CravingBattleInterventionGuide);
        OnPropertyChanged(nameof(CurrentInterventionTitle));
        OnPropertyChanged(nameof(CurrentInterventionIconSource));
        OnPropertyChanged(nameof(CurrentInterventionGuide));

        _totalSeconds = display.DurationSeconds;
        _remainingSeconds = _totalSeconds;
        UpdateTimerDisplay();

        Phase = CravingBattlePhase.Active;
        StartTimer();
    }

    private async Task CompleteAsync()
    {
        CancelTimer();
        var result = await _useCase.CompleteAsync();
        Phase = CravingBattlePhase.Completed;
        StatusMessage = _localization.GetString(LocalizationKeys.CravingBattleCompleted);
    }

    private async Task ResistAsync()
    {
        CancelTimer();
        var result = await _useCase.ResistAsync();
        Phase = CravingBattlePhase.Resisted;
        StatusMessage = _localization.GetString(LocalizationKeys.CravingBattleResisted);
    }

    private async Task ExitAsync()
    {
        CancelTimer();
        var result = await _useCase.ExitSmokedAsync();
        Phase = CravingBattlePhase.Exited;
        StatusMessage = _localization.GetString(LocalizationKeys.CravingBattleExited);
    }

    private void Reset()
    {
        CancelTimer();
        Interventions.Clear();
        Phase = CravingBattlePhase.SelectIntensity;
        StatusMessage = string.Empty;
        TimerText = string.Empty;
        TimerProgress = 0;
        CoachSuggestionText = string.Empty;
        _currentIntervention = null;
        OnPropertyChanged(nameof(CoachSuggestionText));
        OnPropertyChanged(nameof(IsCoachSuggestionVisible));
    }

    private async Task LoadCoachSuggestionAsync(int intensity)
    {
        var request = CoachRequest.Local(new CoachContext(intensity, null, null, null, Array.Empty<string>()));
        var external = await _externalCoachGateway.GenerateAsync(request);
        CoachSuggestionText = external.Succeeded && external.Response is not null
            ? external.Response.Text
            : GetLocalSuggestion(request);
        OnPropertyChanged(nameof(CoachSuggestionText));
        OnPropertyChanged(nameof(IsCoachSuggestionVisible));
    }

    private string GetLocalSuggestion(CoachRequest request)
    {
        var response = LocalDeterministicCoach.Generate(request);
        return response.Suggestions.Count == 0
            ? string.Empty
            : _localization.GetString(response.Suggestions[0].TextKey);
    }

    private void StartTimer()
    {
        _timerCts = new CancellationTokenSource();
        _ = RunTimerAsync(_timerCts.Token);
    }

    private async Task RunTimerAsync(CancellationToken token)
    {
        try
        {
            while (_remainingSeconds > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token).ConfigureAwait(true);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                _remainingSeconds--;
                UpdateTimerDisplay();
            }
        }
        catch (OperationCanceledException)
        {
            // لغو تایمر در بازنشانی/خروج؛ بی‌صدا.
        }
    }

    private void UpdateTimerDisplay()
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo(_localization.CurrentLocale);
        TimerText = string.Format(culture, _localization.GetString(LocalizationKeys.CravingBattleTimer), _remainingSeconds);
        TimerProgress = _totalSeconds == 0 ? 0 : Math.Clamp((double)(_totalSeconds - _remainingSeconds) / _totalSeconds, 0, 1);

        OnPropertyChanged(nameof(TimerText));
        OnPropertyChanged(nameof(TimerProgress));
    }

    private void CancelTimer()
    {
        _timerCts?.Cancel();
        _timerCts?.Dispose();
        _timerCts = null;
    }

    private string IntensityText(CravingIntensity intensity)
    {
        var key = intensity switch
        {
            CravingIntensity.Mild => LocalizationKeys.CravingIntensityMild,
            CravingIntensity.Moderate => LocalizationKeys.CravingIntensityModerate,
            _ => LocalizationKeys.CravingIntensityIntense,
        };

        return _localization.GetString(key);
    }

    private string ActionText(Intervention intervention)
    {
        var key = intervention switch
        {
            Intervention.DeepBreathing => LocalizationKeys.InterventionDeepBreathing,
            Intervention.Delay => LocalizationKeys.InterventionDelay,
            Intervention.DrinkWater => LocalizationKeys.InterventionDrinkWater,
            Intervention.Movement => LocalizationKeys.InterventionMovement,
            _ => LocalizationKeys.InterventionSupportContact,
        };

        return _localization.GetString(key);
    }

    private void PopulateInterventions()
    {
        Interventions.Clear();
        var culture = System.Globalization.CultureInfo.GetCultureInfo(_localization.CurrentLocale);
        foreach (var intervention in Enum.GetValues<Intervention>())
        {
            var durationSeconds = InterventionCatalog.GetDurationSeconds(intervention);
            Interventions.Add(new InterventionDisplay(
                intervention,
                ActionText(intervention),
                durationSeconds,
                string.Format(culture, _localization.GetString(LocalizationKeys.CravingBattleTimer), durationSeconds),
                IconFor(intervention)));
        }
    }

    private static string IconFor(Intervention intervention) => intervention switch
    {
        Intervention.DeepBreathing => "battle_breathe.svg",
        Intervention.Delay => "battle_delay.svg",
        Intervention.DrinkWater => "battle_water.svg",
        Intervention.Movement => "battle_move.svg",
        _ => "battle_support.svg",
    };

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/// <summary>
/// نمای نمایشی یک مداخله (فقط ارائه).
/// </summary>
public sealed record InterventionDisplay(
    Intervention Intervention,
    string Title,
    int DurationSeconds,
    string DurationText,
    string IconSource);
