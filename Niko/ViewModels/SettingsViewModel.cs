// ============================================================================
// Niko.App — SettingsViewModel.cs
// ----------------------------------------------------------------------------
// مسئولیت: مدل نمایش صفحهٔ تنظیمات. فقط مسئول ارائهٔ فرم و قالب‌بندی است؛ تمام
//           اعتبارسنجی و منطق در Core (SaveUserSettingsUseCase) انجام می‌شود.
// وابستگی‌ها و لایه: لایهٔ ارائه (MAUI) → Core (SaveUserSettingsUseCase,
//           ILocalizationService, Domain/UserProfile).
// نکات تغییر و قیود: متن‌های ورودی به‌صورت محلی parse می‌شوند. هیچ متن خامی نداریم.
// ============================================================================

using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Niko.Core.Abstractions;
using Niko.Core.Domain;
using Niko.Core.Domain.Localization;
using Niko.Core.Domain.Settings;
using Niko.Core.Localization;
using Niko.Core.UseCases.Settings;
using Niko.Core.Domain.Coach;
using Niko.Core.UseCases.Coach;
using Niko.Services;

namespace Niko.ViewModels;

/// <summary>
/// مدل نمایش صفحهٔ تنظیمات.
/// </summary>
public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly SaveUserSettingsUseCase _useCase;
    private readonly ILocalizationService _localization;
    private readonly CoachUseCase _coachUseCase;
    private readonly ExternalCoachPrivacyGateway _externalCoachGateway;
    private readonly IWidgetRefreshService _widgetRefresh;

    private string _cigarettesPerDayText = string.Empty;
    private string _pricePerCigaretteText = string.Empty;
    private string _pricePerPackText = string.Empty;
    private string _packSizeText = string.Empty;
    private string _currencyCode = "USD";
    private CurrencyOptionDisplay _selectedCurrency;
    private DateTime? _quitDate;
    private string _statusMessage = string.Empty;
    private string _displayName = string.Empty;
    private AvatarOption _selectedAvatar;
    private LanguageOptionDisplay _selectedLanguage;
    private bool _isLoading;
    private bool _isCoachEnabled;
    private bool _allowExternalProvider;
    private bool _allowAggregatedProgress;
    private bool _allowCravingContext;
    private string _coachStatus = string.Empty;
    private ExternalCoachAvailabilityState _externalAvailabilityState = ExternalCoachAvailabilityState.NotConfigured;

    public SettingsViewModel(
        SaveUserSettingsUseCase useCase,
        ILocalizationService localization,
        CoachUseCase coachUseCase,
        ExternalCoachPrivacyGateway externalCoachGateway,
        IWidgetRefreshService widgetRefresh)
    {
        _useCase = useCase;
        _localization = localization;
        _coachUseCase = coachUseCase;
        _externalCoachGateway = externalCoachGateway;
        _widgetRefresh = widgetRefresh;
        _localization.LocaleChanged += OnLocaleChanged;
        AvatarOptions = AvatarOptionsCatalog.All;
        CurrencyOptions = BuildCurrencyOptions();
        LanguageOptions = SupportedLocales.All
            .Select(option => new LanguageOptionDisplay(
                option,
                _localization.GetString(option.NativeNameKey)))
            .ToList();
        _selectedAvatar = AvatarOptions[0];
        _selectedCurrency = CurrencyOptions.First(option => option.Code == _currencyCode);
        _selectedLanguage = LanguageOptions.First(option =>
            string.Equals(option.Code, _localization.CurrentLocale, StringComparison.OrdinalIgnoreCase) ||
            option.Code.Equals("en", StringComparison.OrdinalIgnoreCase));
        SaveCommand = new Command(async () => await SaveAsync());
        NotificationsCommand = new Command(async () => await Shell.Current.GoToAsync("NotificationsPage"));
        PrivacyDataCommand = new Command(async () => await Shell.Current.GoToAsync("PrivacyDataPage"));
        ClearCoachCommand = new Command(async () => await ClearCoachAsync());
        RevokeExternalCommand = new Command(async () => await RevokeExternalAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => _localization.GetString(LocalizationKeys.SettingsTitle);

    public string ProfileTitle => _localization.GetString(LocalizationKeys.ProfileTitle);

    public string ProfileEntryLabel => _localization.GetString(LocalizationKeys.ProfileEntry);

    public string DisplayNameLabel => _localization.GetString(LocalizationKeys.ProfileDisplayName);

    public string AvatarLabel => _localization.GetString(LocalizationKeys.ProfileAvatar);

    public string AvatarHint => _localization.GetString(LocalizationKeys.ProfileAvatarHint);

    public string LanguageLabel => _localization.GetString(LocalizationKeys.ProfileLanguage);

    public string LanguageFallbackLabel => _localization.GetString(LocalizationKeys.ProfileLanguageFallback);

    public string NotificationsLabel => _localization.GetString(LocalizationKeys.ProfileNotifications);

    public string PrivacyDataLabel => _localization.GetString(LocalizationKeys.ProfilePrivacyData);

    public string CigarettesPerDayLabel => _localization.GetString(LocalizationKeys.SettingsCigarettesPerDay);

    public string PricePerCigaretteLabel => _localization.GetString(LocalizationKeys.SettingsPricePerCigarette);

    public string PricePerPackLabel => _localization.GetString(LocalizationKeys.SettingsPricePerPack);

    public string PackSizeLabel => _localization.GetString(LocalizationKeys.SettingsPackSize);

    public string CurrencyLabel => _localization.GetString(LocalizationKeys.SettingsCurrency);

    public string QuitDateLabel => _localization.GetString(LocalizationKeys.SettingsQuitDate);

    public string SaveLabel => _localization.GetString(LocalizationKeys.SettingsSave);

    public string SavingsHint => _localization.GetString(LocalizationKeys.SettingsSavingsHint);

    public string CoachTitle => _localization.GetString(LocalizationKeys.CoachTitle);

    public string CoachPrivacyNote => _localization.GetString(LocalizationKeys.CoachPrivacyNote);

    public string CoachDisabledText => _localization.GetString(LocalizationKeys.CoachDisabled);

    public string CoachEnabledText => _localization.GetString(LocalizationKeys.CoachEnabled);

    public string CoachAllowExternalLabel => _localization.GetString(LocalizationKeys.CoachAllowExternal);

    public string CoachAllowProgressLabel => _localization.GetString(LocalizationKeys.CoachAllowProgress);

    public string CoachAllowCravingLabel => _localization.GetString(LocalizationKeys.CoachAllowCraving);

    public string CoachClearLabel => _localization.GetString(LocalizationKeys.CoachClear);

    public string CoachStatus
    {
        get => _coachStatus;
        private set => SetField(ref _coachStatus, value);
    }

    public string CoachExternalTitle => _localization.GetString(LocalizationKeys.CoachExternalTitle);

    public string CoachExternalNote => _localization.GetString(LocalizationKeys.CoachExternalNote);

    public string CoachExternalConsentLabel => _localization.GetString(LocalizationKeys.CoachExternalConsent);

    public string CoachExternalUnavailable => _localization.GetString(LocalizationKeys.CoachExternalUnavailable);

    public string CoachExternalStatus => _externalAvailabilityState == ExternalCoachAvailabilityState.AvailableFree
        ? _localization.GetString(LocalizationKeys.CoachExternalAvailable)
        : _localization.GetString(LocalizationKeys.CoachExternalUnavailable);

    public string CoachExternalRevokeLabel => _localization.GetString(LocalizationKeys.CoachExternalRevoke);

    public string CigarettesPerDayText
    {
        get => _cigarettesPerDayText;
        set => SetField(ref _cigarettesPerDayText, value);
    }

    public string PricePerCigaretteText
    {
        get => _pricePerCigaretteText;
        set => SetField(ref _pricePerCigaretteText, value);
    }

    public string PricePerPackText
    {
        get => _pricePerPackText;
        set => SetField(ref _pricePerPackText, value);
    }

    public string PackSizeText
    {
        get => _packSizeText;
        set => SetField(ref _packSizeText, value);
    }

    public string CurrencyCode
    {
        get => _currencyCode;
        set => SetField(ref _currencyCode, value);
    }

    public IReadOnlyList<CurrencyOptionDisplay> CurrencyOptions { get; }

    public CurrencyOptionDisplay SelectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (SetField(ref _selectedCurrency, value))
            {
                CurrencyCode = value.Code;
            }
        }
    }

    public DateTime? QuitDate
    {
        get => _quitDate;
        set => SetField(ref _quitDate, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public Command SaveCommand { get; }

    public Command NotificationsCommand { get; }

    public Command PrivacyDataCommand { get; }

    public Command ClearCoachCommand { get; }

    public Command RevokeExternalCommand { get; }

    public bool IsCoachEnabled
    {
        get => _isCoachEnabled;
        set
        {
            if (SetField(ref _isCoachEnabled, value))
            {
                OnPropertyChanged(nameof(IsCoachDisabled));
            }
        }
    }

    public bool IsCoachDisabled => !IsCoachEnabled;

    public bool AllowExternalProvider
    {
        get => _allowExternalProvider;
        set => SetField(ref _allowExternalProvider, value && IsExternalProviderAvailable);
    }

    public ExternalCoachAvailabilityState ExternalAvailabilityState
    {
        get => _externalAvailabilityState;
        private set
        {
            if (SetField(ref _externalAvailabilityState, value))
            {
                OnPropertyChanged(nameof(IsExternalProviderAvailable));
                OnPropertyChanged(nameof(CoachExternalStatus));
                if (!IsExternalProviderAvailable)
                {
                    AllowExternalProvider = false;
                }
            }
        }
    }

    public bool IsExternalProviderAvailable => ExternalAvailabilityState == ExternalCoachAvailabilityState.AvailableFree;

    public bool AllowAggregatedProgress
    {
        get => _allowAggregatedProgress;
        set => SetField(ref _allowAggregatedProgress, value);
    }

    public bool AllowCravingContext
    {
        get => _allowCravingContext;
        set => SetField(ref _allowCravingContext, value);
    }

    public IReadOnlyList<AvatarOption> AvatarOptions { get; }

    public IReadOnlyList<LanguageOptionDisplay> LanguageOptions { get; }

    public string DisplayName
    {
        get => _displayName;
        set => SetField(ref _displayName, value);
    }

    public AvatarOption SelectedAvatar
    {
        get => _selectedAvatar;
        set
        {
            if (SetField(ref _selectedAvatar, value))
            {
                OnPropertyChanged(nameof(SelectedAvatarSymbol));
            }
        }
    }

    public string SelectedAvatarSymbol => SelectedAvatar.Symbol;

    public LanguageOptionDisplay SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (!SetField(ref _selectedLanguage, value) || _isLoading)
            {
                return;
            }

            _ = ChangeLanguageAsync(value);
        }
    }

    public bool IsLanguageFallback => !SelectedLanguage.IsFullyTranslated;

    private void OnLocaleChanged(object? sender, EventArgs e)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

    /// <summary>بارگذاری تنظیمات ذخیره‌شده از Core (فقط ارائه).</summary>
    public async Task LoadAsync()
    {
        _isLoading = true;
        var profile = await _useCase.LoadAsync();
        if (profile is not null)
        {
            DisplayName = profile.DisplayName ?? string.Empty;
            SelectedAvatar = AvatarOptions.FirstOrDefault(option => option.Id == profile.AvatarId) ?? AvatarOptions[0];
            CigarettesPerDayText = profile.CigarettesPerDay?.ToString() ?? string.Empty;
            PricePerCigaretteText = profile.PricePerCigarette?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            PricePerPackText = profile.PricePerPack?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            PackSizeText = profile.PackSize?.ToString() ?? string.Empty;
            CurrencyCode = profile.CurrencyCode;
            SelectedCurrency = CurrencyOptions.FirstOrDefault(option =>
                string.Equals(option.Code, profile.CurrencyCode, StringComparison.OrdinalIgnoreCase))
                ?? CurrencyOptions[0];
            QuitDate = profile.QuitDateUtc?.LocalDateTime.Date;
            SelectedLanguage = LanguageOptions.FirstOrDefault(option =>
                string.Equals(option.Code, profile.PreferredLocale, StringComparison.OrdinalIgnoreCase))
                ?? LanguageOptions.First(option => option.Code == "en");
        }

        var coach = await _coachUseCase.GetPreferencesAsync();
        ExternalAvailabilityState = (await _externalCoachGateway.GetAvailabilityAsync()).State;
        IsCoachEnabled = coach.Enabled;
        AllowExternalProvider = coach.AllowExternalProvider && IsExternalProviderAvailable;
        AllowAggregatedProgress = coach.AllowAggregatedProgressContext;
        AllowCravingContext = coach.AllowCravingContext;
        StatusMessage = string.Empty;
        _isLoading = false;
    }

    private async Task SaveAsync()
    {
        var profile = BuildProfile();
        var result = await _useCase.SaveAsync(profile);

        await _coachUseCase.SetPreferencesAsync(new CoachPreferences
        {
            Enabled = IsCoachEnabled,
            AllowExternalProvider = AllowExternalProvider,
            AllowAggregatedProgressContext = AllowAggregatedProgress,
            AllowCravingContext = AllowCravingContext,
        });

        StatusMessage = result.IsValid
            ? _localization.GetString(LocalizationKeys.ProfileSaved)
            : _localization.GetString(ErrorKey(result.Error ?? UserSettingsValidationResult.InvalidCigarettesPerDay));
    }

    private async Task ClearCoachAsync()
    {
        await _coachUseCase.ClearCoachDataAsync();
        IsCoachEnabled = false;
        AllowExternalProvider = false;
        AllowAggregatedProgress = false;
        AllowCravingContext = false;
        CoachStatus = _localization.GetString(LocalizationKeys.CoachCleared);
    }

    private async Task RevokeExternalAsync()
    {
        AllowExternalProvider = false;
        await _coachUseCase.SetPreferencesAsync(new CoachPreferences
        {
            Enabled = IsCoachEnabled,
            AllowExternalProvider = false,
            AllowAggregatedProgressContext = AllowAggregatedProgress,
            AllowCravingContext = AllowCravingContext,
        });
        CoachStatus = _localization.GetString(LocalizationKeys.CoachExternalRevoked);
    }

    private async Task ChangeLanguageAsync(LanguageOptionDisplay locale)
    {
        if (!await _useCase.SavePreferredLocaleAsync(locale.Code))
        {
            StatusMessage = _localization.GetString(LocalizationKeys.ProfileErrorLocale);
            return;
        }

        _localization.SetLocale(locale.Code);
        await _widgetRefresh.RequestRefreshAsync();
        OnPropertyChanged(nameof(IsLanguageFallback));
    }

    private UserProfile BuildProfile()
    {
        return new UserProfile
        {
            DisplayName = string.IsNullOrWhiteSpace(DisplayName) ? null : DisplayName.Trim(),
            AvatarId = SelectedAvatar.Id,
            QuitDateUtc = QuitDate is { } d
                ? new DateTimeOffset(DateTime.SpecifyKind(d, DateTimeKind.Utc))
                : null,
            CigarettesPerDay = ParseInt(CigarettesPerDayText),
            PricePerCigarette = ParseDecimal(PricePerCigaretteText),
            PricePerPack = ParseDecimal(PricePerPackText),
            PackSize = ParseInt(PackSizeText),
            CurrencyCode = SelectedCurrency.Code,
            PreferredLocale = SelectedLanguage.Code,
        };
    }

    private static int? ParseInt(string? text)
        => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static decimal? ParseDecimal(string? text)
        => decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : null;

    private string ErrorKey(UserSettingsValidationResult error)
    {
        return error switch
        {
            UserSettingsValidationResult.InvalidCigarettesPerDay => LocalizationKeys.SettingsErrorCigarettesPerDay,
            UserSettingsValidationResult.InvalidPrice => LocalizationKeys.SettingsErrorPrice,
            UserSettingsValidationResult.InvalidPackSize => LocalizationKeys.SettingsErrorPackSize,
            UserSettingsValidationResult.MissingPrice => LocalizationKeys.SettingsErrorMissingPrice,
            UserSettingsValidationResult.InvalidCurrency => LocalizationKeys.SettingsErrorCurrency,
            UserSettingsValidationResult.InvalidDisplayName => LocalizationKeys.ProfileErrorDisplayName,
            UserSettingsValidationResult.InvalidAvatar => LocalizationKeys.ProfileErrorAvatar,
            UserSettingsValidationResult.InvalidLocale => LocalizationKeys.ProfileErrorLocale,
            _ => LocalizationKeys.SettingsErrorQuitDate,
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static class AvatarOptionsCatalog
    {
        public static IReadOnlyList<AvatarOption> All => Niko.Core.Domain.Settings.AvatarOptions.All;
    }

    public sealed record LanguageOptionDisplay(
        SupportedLocale Locale,
        string DisplayName)
    {
        public string Code => Locale.Code;

        public bool IsFullyTranslated => Locale.IsFullyTranslated;

        public bool IsRightToLeft => Locale.IsRightToLeft;
    }

    public sealed record CurrencyOptionDisplay(string Code, string DisplayName);

    private static IReadOnlyList<CurrencyOptionDisplay> BuildCurrencyOptions()
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            try
            {
                var region = new RegionInfo(culture.Name);
                if (!string.IsNullOrWhiteSpace(region.ISOCurrencySymbol))
                {
                    options.TryAdd(
                        region.ISOCurrencySymbol.ToUpperInvariant(),
                        region.CurrencyEnglishName);
                }
            }
            catch (CultureNotFoundException)
            {
                // Some platform culture entries are metadata-only.
            }
        }

        options.TryAdd("USD", "US Dollar");
        return options
            .OrderBy(option => option.Key, StringComparer.Ordinal)
            .Select(option => new CurrencyOptionDisplay(option.Key, $"{option.Key} — {option.Value}"))
            .ToList();
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
