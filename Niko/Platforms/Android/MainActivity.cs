// ============================================================================
// Niko.App — MainActivity.cs
// ----------------------------------------------------------------------------
// مسئولیت: میزبان چرخهٔ عمر MAUI و بازگرداندن نتیجهٔ تأیید قفل دستگاه.
// وابستگی‌ها و لایه: Android Activity → AndroidDeviceCredentialConfirmation؛ بدون منطق دامنه.
// نکات تغییر و قیود: فقط نتیجهٔ موفق دستگاه اجازهٔ حذف داده را به UI برمی‌گرداند.
// ============================================================================

using Android.App;
using Android.Content;
using Android.Content.PM;
using Niko.Platforms.Android.Privacy;

namespace Niko;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        AndroidDeviceCredentialConfirmation.Complete(requestCode, resultCode);
    }
}
