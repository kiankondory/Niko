using Microsoft.Extensions.DependencyInjection;

namespace Niko
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services, Niko.Services.IAppThemeService appThemeService)
        {
            InitializeComponent();
            _services = services;
            appThemeService.ApplyStoredTheme();

            var profile = _services.GetRequiredService<Niko.Core.Abstractions.IUserSettingsStore>()
                .GetAsync()
                .GetAwaiter()
                .GetResult();
            if (!string.IsNullOrWhiteSpace(profile?.PreferredLocale))
            {
                _services.GetRequiredService<Niko.Core.Abstractions.ILocalizationService>()
                    .SetLocale(profile.PreferredLocale);
            }

            Routing.RegisterRoute("CravingBattlePage", typeof(Pages.CravingBattlePage));
            Routing.RegisterRoute("SettingsPage", typeof(Pages.SettingsPage));
            Routing.RegisterRoute("NotificationsPage", typeof(Pages.NotificationsPage));
            Routing.RegisterRoute("ProfilePage", typeof(Pages.ProfilePage));
            Routing.RegisterRoute("PrivacyDataPage", typeof(Pages.PrivacyDataPage));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = _services.GetRequiredService<AppShell>();
            return new Window(shell);
        }
    }
}
