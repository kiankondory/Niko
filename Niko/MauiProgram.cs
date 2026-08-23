using Microsoft.Extensions.Logging;
using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;
using Niko.Core.Sync;
using Niko.Core.UseCases.Companion;
using Niko.Core.UseCases.Coach;
using Niko.Core.UseCases.CravingBattle;
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.Notifications;
using Niko.Core.UseCases.QuickLog;
using Niko.Core.UseCases.Settings;
using Niko.Core.UseCases.TriggerAnalysis;
using Niko.Core.UseCases.Privacy;
using Niko.Infrastructure.Persistence;
using Niko.Infrastructure.Coach;
using Niko.Services;

namespace Niko
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // لایهٔ Core و Infrastructure
            builder.Services.AddSingleton<IClock, SystemClock>();

            builder.Services.AddSingleton<ILocalStore>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "niko.db");
                return new SqliteStore(path);
            });

            builder.Services.AddSingleton<IUserSettingsStore>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "niko.db");
                return new UserSettingsStore(path);
            });

            builder.Services.AddSingleton<INotificationPreferencesStore>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "niko.db");
                return new NotificationPreferencesStore(path);
            });

            builder.Services.AddSingleton<ITriggerAnalysisPreferenceStore>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "niko.db");
                return new TriggerAnalysisPreferenceStore(path);
            });

            builder.Services.AddSingleton<ICoachPreferencesStore>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "niko.db");
                return new CoachPreferencesStore(path);
            });
            builder.Services.AddSingleton<IPrivacyDataStore>(sp => new SqlitePrivacyDataStore(Path.Combine(FileSystem.AppDataDirectory, "niko.db")));

            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            builder.Services.AddSingleton<IAppThemeService, AppThemeService>();
            builder.Services.AddSingleton<IAppMotionService, AppMotionService>();
            builder.Services.AddSingleton<IDeviceConfirmationService>(sp =>
#if ANDROID
                new Platforms.Android.Privacy.AndroidDeviceConfirmationService()
#else
                new UnavailableDeviceConfirmationService()
#endif
            );
            builder.Services.AddSingleton<IFeatureFlagProvider, EnvironmentFeatureFlagProvider>();
            builder.Services.AddSingleton<IWidgetRefreshService>(sp =>
#if ANDROID
                new Platforms.Android.Widget.AndroidWidgetRefreshService()
#else
                new NoopWidgetRefreshService()
#endif
            );
            builder.Services.AddSingleton<ISyncTransport>(_ => new NoopSyncTransport());
            builder.Services.AddSingleton<SyncQueue>();

            builder.Services.AddSingleton<INotificationService>(sp =>
#if ANDROID
                new AndroidNotificationService(
                    sp.GetRequiredService<ILocalizationService>())
#else
                new NoopNotificationService()
#endif
            );
            builder.Services.AddSingleton<NotificationSettingsUseCase>();

            builder.Services.AddSingleton<QuickLogUseCase>();
            builder.Services.AddSingleton<DashboardUseCase>();
            builder.Services.AddSingleton<CravingBattleUseCase>();
            builder.Services.AddSingleton<SaveUserSettingsUseCase>();
            builder.Services.AddSingleton<TriggerAnalysisUseCase>();
            builder.Services.AddSingleton<CoachUseCase>();
            builder.Services.AddSingleton<PrivacyDataUseCase>();
            builder.Services.AddSingleton<IExternalCoachProvider>(sp =>
            {
                var endpoint = Environment.GetEnvironmentVariable("COACH_PROXY_URL");
                var healthEndpoint = Environment.GetEnvironmentVariable("COACH_PROXY_HEALTH_URL");
                var token = Environment.GetEnvironmentVariable("COACH_PROXY_SESSION_TOKEN");
                return new BackendCoachProxyProvider(new HttpClient(), endpoint, healthEndpoint, token);
            });
            builder.Services.AddSingleton<ExternalCoachPrivacyGateway>(sp =>
                new ExternalCoachPrivacyGateway(
                    sp.GetRequiredService<ICoachPreferencesStore>(),
                    sp.GetRequiredService<IExternalCoachProvider>(),
                    new ExternalCoachProviderConfiguration(
                        Enabled: !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("COACH_PROXY_URL")),
                        BillingExplicitlyDisabled: true,
                        PaidFallbackConfigured: false)));

            builder.Services.AddSingleton<IProcessedMessageStore>(sp =>
            {
                var path = Path.Combine(FileSystem.AppDataDirectory, "niko.db");
                return new SqliteProcessedMessageStore(path);
            });
            builder.Services.AddSingleton<CompanionUseCase>();

            builder.Services.AddSingleton<ViewModels.MainViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<ViewModels.DashboardViewModel>();
            builder.Services.AddSingleton<Pages.DashboardPage>();
            builder.Services.AddSingleton<ViewModels.CravingBattleViewModel>();
            builder.Services.AddSingleton<Pages.CravingBattlePage>();
            builder.Services.AddSingleton<ViewModels.IslandViewModel>();
            builder.Services.AddSingleton<Pages.IslandPage>();
            builder.Services.AddSingleton<ViewModels.SettingsViewModel>();
            builder.Services.AddSingleton<Pages.SettingsPage>();
            builder.Services.AddSingleton<Pages.ProfilePage>();
            // هر پاک‌سازی داده باید onboarding و Shell تازه داشته باشد تا history
            // مسیرهای قبلی (مثل Privacy) به profile حذف‌شده نشت نکند.
            builder.Services.AddTransient<Pages.OnboardingPage>();
            builder.Services.AddTransient<Pages.PrivacyDataPage>();
            builder.Services.AddSingleton<ViewModels.NotificationsViewModel>();
            builder.Services.AddSingleton<Pages.NotificationsPage>();
            builder.Services.AddTransient<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
