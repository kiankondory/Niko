// ============================================================================
// Niko.App — OnboardingPage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: کنترل معرفی اولین اجرا، ذخیرهٔ locale انتخاب‌شده و ورود امن به Shell.
// وابستگی‌ها و لایه: MAUI presentation → SaveUserSettingsUseCase/ILocalizationService.
// نکات تغییر و قیود: فقط preference ارائه‌ای پایان onboarding می‌نویسد و هیچ event
//           یا دادهٔ سلامت ایجاد نمی‌کند؛ locale با مسیر SQLite موجود ذخیره می‌شود.
// ============================================================================

using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Localization;
using Niko.Core.Localization;
using Niko.Core.UseCases.Settings;

namespace Niko.Pages;

public partial class OnboardingPage : ContentPage
{
    internal const string CompletionPreferenceKey = "niko.onboarding.completed";
    private readonly SaveUserSettingsUseCase _settings;
    private readonly ILocalizationService _localization;
    private readonly AppShell _shell;
    private int _step;
    private string _selectedLocale = "en";

    public OnboardingPage(SaveUserSettingsUseCase settings, ILocalizationService localization, AppShell shell)
    {
        InitializeComponent();
        _settings = settings;
        _localization = localization;
        _shell = shell;
        LanguageOptions = SupportedLocales.All
            .Select(locale => new LocaleOption(locale, _localization.GetString(locale.NativeNameKey)))
            .ToList();
        Steps = new ObservableCollection<OnboardingStepDisplay>();
        Feed.BindingContext = this;
        Render();
    }

    public IReadOnlyList<LocaleOption> LanguageOptions { get; }

    public ObservableCollection<OnboardingStepDisplay> Steps { get; }

    private async void OnNextClicked(object? sender, EventArgs eventArgs)
    {
        if (_step == 0)
        {
            if (!await _settings.SavePreferredLocaleAsync(_selectedLocale)) return;
            _localization.SetLocale(_selectedLocale);
        }
        if (_step >= 6)
        {
            Preferences.Default.Set(CompletionPreferenceKey, true);
            Application.Current?.Windows.FirstOrDefault()?.Page = _shell;
            return;
        }
        _step++;
        Feed.Position = _step;
        UpdateButton();
    }

    private void Render()
    {
        BrandLabel.Text = _localization.GetString(LocalizationKeys.AppTitle);
        var steps = new (string Title, string Body)[]
        {
            (LocalizationKeys.OnboardingLanguageTitle, LocalizationKeys.OnboardingLanguageBody),
            (LocalizationKeys.OnboardingWelcomeTitle, LocalizationKeys.OnboardingWelcomeBody),
            (LocalizationKeys.OnboardingHomeTitle, LocalizationKeys.OnboardingHomeBody),
            (LocalizationKeys.OnboardingLogTitle, LocalizationKeys.OnboardingLogBody),
            (LocalizationKeys.OnboardingBattleTitle, LocalizationKeys.OnboardingBattleBody),
            (LocalizationKeys.OnboardingIslandTitle, LocalizationKeys.OnboardingIslandBody),
            (LocalizationKeys.OnboardingSettingsTitle, LocalizationKeys.OnboardingSettingsBody),
        };
        Steps.Clear();
        for (var index = 0; index < steps.Length; index++)
        {
            Steps.Add(new OnboardingStepDisplay(
                _localization.GetString(steps[index].Title),
                _localization.GetString(steps[index].Body),
                index == 0 ? "🌐" : index == 1 ? "✦" : index == 2 ? "⌂" : index == 3 ? "✓" : index == 4 ? "⚔" : index == 5 ? "🏝" : "⚙",
                index == 0));
        }
        UpdateButton();
    }

    private void OnFeedPositionChanged(object? sender, PositionChangedEventArgs eventArgs)
    {
        _step = eventArgs.CurrentPosition;
        UpdateButton();
    }

    private void OnLanguageChanged(object? sender, EventArgs eventArgs)
    {
        if (sender is Picker picker && picker.SelectedItem is LocaleOption option)
        {
            _selectedLocale = option.Code;
            _localization.SetLocale(_selectedLocale);
            Render();
        }
    }

    private void UpdateButton()
        => NextButton.Text = _localization.GetString(
            _step == 6 ? LocalizationKeys.OnboardingStart : LocalizationKeys.OnboardingNext);

    public sealed record OnboardingStepDisplay(string Title, string Body, string Icon, bool IsLanguageStep);

    public sealed record LocaleOption(SupportedLocale Locale, string DisplayName)
    {
        public string Code => Locale.Code;
    }
}
