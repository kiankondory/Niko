// مسئولیت: دریافت پیام QuickLog از Wear OS و واگذاری آن به CompanionUseCase.
// وابستگی‌ها و لایه: Android Data Layer adapter → Core CompanionUseCase؛ هیچ
// منطق دامنه، storage موازی، شبکه یا دادهٔ خصوصی در این listener وجود ندارد.
// نکات تغییر و قیود: پیام خام فقط پس از دریافت از مسیر نسخه‌دار به Core سپرده
// می‌شود؛ اعتبارسنجی، deduplication و ثبت رویداد کاملاً در Core انجام می‌شود.

using System.Text;
using Android.App;
using Android.Gms.Wearable;
using Microsoft.Maui;
using Niko.Core.UseCases.Companion;

namespace Niko.Platforms.Android.Wear;

[Service(Exported = true, Name = "com.companyname.niko.WearMessageListenerService")]
[IntentFilter(new[] { "com.google.android.gms.wearable.MESSAGE_RECEIVED" })]
public sealed class WearMessageListenerService : WearableListenerService
{
    public const string QuickLogPath = "/niko/companion/quicklog";

    public override async void OnMessageReceived(IMessageEvent messageEvent)
    {
        if (!string.Equals(messageEvent.Path, QuickLogPath, StringComparison.Ordinal))
        {
            return;
        }

        var services = IPlatformApplication.Current?.Services;
        var companion = services?.GetService<CompanionUseCase>();
        if (companion is null)
        {
            return;
        }

        var serializedMessage = Encoding.UTF8.GetString(messageEvent.GetData());
        await companion.HandleAsync(serializedMessage).ConfigureAwait(false);
    }
}
