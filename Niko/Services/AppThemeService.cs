// ============================================================================
// Niko.App — AppThemeService.cs
// ----------------------------------------------------------------------------
// مسئولیت: نگهداری و اعمال ترجیح بصری Light/Dark/System برای MAUI بدون ورود به
//           مدل دامنه یا داده‌های سلامت کاربر.
// وابستگی‌ها و لایه: Presentation service → Microsoft.Maui.Storage/Controls؛
//           مستقل از Core، SQLite و رویدادهای کاربر.
// نکات تغییر و قیود: فقط یک تنظیم ظاهری محلی را persist می‌کند؛ به هیچ سرویس
//           خارجی ارسال نمی‌شود و تغییر آن دادهٔ دامنه را تغییر نمی‌دهد.
// ============================================================================

using Microsoft.Maui.Storage;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace Niko.Services;

public enum AppThemeMode
{
    System = 0,
    Light = 1,
    Dark = 2,
}

public interface IAppThemeService
{
    AppThemeMode Current { get; }

    void ApplyStoredTheme();

    void SetTheme(AppThemeMode mode);
}

public sealed class AppThemeService : IAppThemeService
{
    private const string PreferenceKey = "niko.app-theme";

    public AppThemeMode Current { get; private set; } = AppThemeMode.System;

    public void ApplyStoredTheme()
    {
        var saved = Preferences.Default.Get(PreferenceKey, AppThemeMode.System.ToString());
        Current = Enum.TryParse<AppThemeMode>(saved, ignoreCase: true, out var mode)
            ? mode
            : AppThemeMode.System;
        Apply(Current);
    }

    public void SetTheme(AppThemeMode mode)
    {
        Current = mode;
        Preferences.Default.Set(PreferenceKey, mode.ToString());
        Apply(mode);
    }

    private static void Apply(AppThemeMode mode)
    {
        if (MauiApplication.Current is null)
        {
            return;
        }

        MauiApplication.Current.UserAppTheme = mode switch
        {
            AppThemeMode.Light => AppTheme.Light,
            AppThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
