// ============================================================================
// Niko.App — App.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: ساخت سریع پنجرهٔ اصلی و اعمال غیرمسدودکنندهٔ locale/onboarding ذخیره‌شده.
// وابستگی‌ها و لایه: MAUI composition → IUserSettingsStore و ILocalizationService.
// نکات تغییر و قیود: نبود profile محلی onboarding را حتی پس از بازگردانی ناقص preference
//           فعال می‌کند؛ خواندن SQLite هرگز UI thread را پیش از ساخت Window مسدود نمی‌کند.
// ============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Niko.Pages;

namespace Niko
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<App> _logger;

        public App(
            IServiceProvider services,
            Niko.Services.IAppThemeService appThemeService,
            ILogger<App> logger)
        {
            InitializeComponent();
            _services = services;
            _logger = logger;
            appThemeService.ApplyStoredTheme();
            _services.GetRequiredService<Niko.Services.IAppMotionService>().ApplyStoredPreference();

            Routing.RegisterRoute("CravingBattlePage", typeof(Pages.CravingBattlePage));
            Routing.RegisterRoute("SettingsPage", typeof(Pages.SettingsPage));
            Routing.RegisterRoute("NotificationsPage", typeof(Pages.NotificationsPage));
            Routing.RegisterRoute("ProfilePage", typeof(Pages.ProfilePage));
            Routing.RegisterRoute("PrivacyDataPage", typeof(Pages.PrivacyDataPage));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (!Preferences.Default.Get(OnboardingPage.CompletionPreferenceKey, false))
            {
                return new Window(_services.GetRequiredService<OnboardingPage>());
            }
            var shell = _services.GetRequiredService<AppShell>();
            var window = new Window(shell);
            _ = ApplyStoredLocaleOrShowOnboardingAsync(window);
            return window;
        }

        /// <summary>
        /// پس از حذف موفق داده‌ها، پنجره را با یک onboarding تازه جایگزین می‌کند تا
        /// Shell و مسیرهای قبلیِ وابسته به profile حذف‌شده باقی نمانند.
        /// </summary>
        public void RestartOnboardingAfterDataErasure()
        {
            Preferences.Default.Remove(OnboardingPage.CompletionPreferenceKey);
            var window = Windows.FirstOrDefault();
            if (window is not null)
            {
                window.Page = _services.GetRequiredService<OnboardingPage>();
            }
        }

        private async Task ApplyStoredLocaleOrShowOnboardingAsync(Window window)
        {
            try
            {
                var profile = await _services
                    .GetRequiredService<Niko.Core.Abstractions.IUserSettingsStore>()
                    .GetAsync();
                var locale = profile?.PreferredLocale;
                if (profile is null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Preferences.Default.Remove(OnboardingPage.CompletionPreferenceKey);
                        window.Page = _services.GetRequiredService<OnboardingPage>();
                    });
                    return;
                }

                if (!string.IsNullOrWhiteSpace(locale))
                {
                    MainThread.BeginInvokeOnMainThread(() => _services
                        .GetRequiredService<Niko.Core.Abstractions.ILocalizationService>()
                        .SetLocale(locale));
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    "خواندن locale ذخیره‌شده در startup ناموفق بود. نوع خطا: {ExceptionType}",
                    exception.GetType().Name);
            }
        }
    }
}
