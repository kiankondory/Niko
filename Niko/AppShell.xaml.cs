using Microsoft.Extensions.DependencyInjection;
using Niko.Core.Abstractions;
using Niko.Core.Localization;
using Niko.Pages;

namespace Niko
{
    public partial class AppShell : Shell
    {
        public AppShell(IServiceProvider services)
        {
            InitializeComponent();

            var localization = services.GetRequiredService<ILocalizationService>();

            // تب‌ها با DataTemplate مبتنی بر DI ساخته می‌شوند و عنوانشان محلی‌سازی می‌شود.
            DashboardShellContent.Title = localization.GetString(LocalizationKeys.TabHome);
            DashboardShellContent.ContentTemplate = new DataTemplate(() =>
                services.GetRequiredService<DashboardPage>());

            MainShellContent.Title = localization.GetString(LocalizationKeys.TabQuickLog);
            MainShellContent.ContentTemplate = new DataTemplate(() =>
                services.GetRequiredService<MainPage>());

            BattleShellContent.Title = localization.GetString(LocalizationKeys.TabBattle);
            BattleShellContent.ContentTemplate = new DataTemplate(() =>
                services.GetRequiredService<CravingBattlePage>());

            IslandShellContent.Title = localization.GetString(LocalizationKeys.TabIsland);
            IslandShellContent.ContentTemplate = new DataTemplate(() =>
                services.GetRequiredService<IslandPage>());

            ProfileShellContent.Title = localization.GetString(LocalizationKeys.ProfileTitle);
            ProfileShellContent.ContentTemplate = new DataTemplate(() =>
                services.GetRequiredService<ProfilePage>());

            localization.LocaleChanged += (_, _) =>
            {
                DashboardShellContent.Title = localization.GetString(LocalizationKeys.TabHome);
                MainShellContent.Title = localization.GetString(LocalizationKeys.TabQuickLog);
                BattleShellContent.Title = localization.GetString(LocalizationKeys.TabBattle);
                IslandShellContent.Title = localization.GetString(LocalizationKeys.TabIsland);
                ProfileShellContent.Title = localization.GetString(LocalizationKeys.ProfileTitle);
                FlowDirection = IsRtl(localization.CurrentLocale)
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
            };

            FlowDirection = IsRtl(localization.CurrentLocale)
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }

        private static bool IsRtl(string locale)
            => locale.StartsWith("fa", StringComparison.OrdinalIgnoreCase) ||
               locale.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
    }
}
