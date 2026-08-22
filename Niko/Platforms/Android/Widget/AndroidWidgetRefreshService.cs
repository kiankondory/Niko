// ============================================================================
// Niko.App — AndroidWidgetRefreshService.cs
// ----------------------------------------------------------------------------
// مسئولیت: درخواست بازخوانی Provider ابزارک Android پس از ثبت موفق QuickLog.
// وابستگی‌ها و لایه: Android adapter → IWidgetRefreshService و
//           NikoWidgetProvider؛ هیچ شمارش یا ذخیره‌سازی موازی انجام نمی‌دهد.
// نکات تغییر و قیود: Provider دوباره از CompanionUseCase خلاصهٔ محلی را می‌خواند؛
//           این آداپتر فقط broadcast امن و بدون دادهٔ خصوصی ارسال می‌کند.
// ============================================================================

using Android.App;
using Android.Appwidget;
using Android.Content;
using Niko.Services;

namespace Niko.Platforms.Android.Widget;

public sealed class AndroidWidgetRefreshService : IWidgetRefreshService
{
    public Task RequestRefreshAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var context = global::Android.App.Application.Context;
        var manager = AppWidgetManager.GetInstance(context);
        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(NikoWidgetProvider)));
        var ids = manager?.GetAppWidgetIds(component) ?? Array.Empty<int>();
        if (ids.Length > 0)
        {
            var intent = new Intent(context, typeof(NikoWidgetProvider));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, ids);
            context.SendBroadcast(intent);
        }

        return Task.CompletedTask;
    }
}
