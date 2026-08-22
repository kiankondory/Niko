// ============================================================================
// Niko.App — NikoWidgetProvider.cs
// ----------------------------------------------------------------------------
// مسئولیت: آداپتر بومی Android برای نمایش خلاصهٔ امن و ارسال سه عمل QuickLog.
// وابستگی‌ها و لایه: Android adapter → CompanionMessage/CompanionUseCase/Core و
//           SqliteStore؛ هیچ منطق دامنه یا ذخیره‌سازی موازی در ابزارک وجود ندارد.
// نکات تغییر و قیود: پیام‌ها نسخه‌بندی‌شده و با MessageId یکتا هستند؛ خطا به
//           وضعیت امن ابزارک تبدیل می‌شود و متن‌ها از منابع Android می‌آیند.
// ============================================================================

using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.Res;
using Android.Widget;
using Java.Util;
using Niko.Core.Abstractions;
using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;
using Niko.Core.UseCases.Companion;
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.QuickLog;
using Niko.Infrastructure.Persistence;

namespace Niko.Platforms.Android.Widget;

[BroadcastReceiver(Name = "com.companyname.niko.NikoWidgetProvider", Enabled = true, Exported = true)]
[IntentFilter(new[] { AppWidgetManager.ActionAppwidgetUpdate })]
public sealed class NikoWidgetProvider : AppWidgetProvider
{
    private const string ActionQuickLog = "Niko.Widget.QuickLog";
    private const string EventTypeExtra = "event_type";

    public override void OnUpdate(Context? context, AppWidgetManager? manager, int[]? ids)
    {
        if (context is null || manager is null || ids is null)
        {
            return;
        }

        var pending = GoAsync();
        if (pending is null)
        {
            return;
        }
        _ = RefreshAsync(context, manager, ids, pending);
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        base.OnReceive(context, intent);
        if (context is null || intent?.Action != ActionQuickLog)
        {
            return;
        }

        var pending = GoAsync();
        if (pending is null)
        {
            return;
        }
        _ = ProcessQuickLogAsync(context, intent, pending);
    }

    private static async Task ProcessQuickLogAsync(Context context, Intent intent, PendingResult pending)
    {
        try
        {
            var type = (EventType)intent.GetIntExtra(EventTypeExtra, -1);
            if (type is not (EventType.Smoked or EventType.Resisted or EventType.Craving))
            {
                return;
            }

            var bridge = new WidgetCompanionBridge(context);
            await bridge.HandleQuickLogAsync(type).ConfigureAwait(false);
            await bridge.RefreshAllAsync().ConfigureAwait(false);
        }
        finally
        {
            pending.Finish();
        }
    }

    private static async Task RefreshAsync(Context context, AppWidgetManager manager, int[] ids, PendingResult pending)
    {
        try
        {
            await new WidgetCompanionBridge(context).RefreshAsync(manager, ids).ConfigureAwait(false);
        }
        finally
        {
            pending.Finish();
        }
    }

    private static PendingIntent CreateAction(Context context, EventType type)
    {
        var intent = new Intent(context, typeof(NikoWidgetProvider));
        intent.SetAction(ActionQuickLog);
        intent.PutExtra(EventTypeExtra, (int)type);
        var flags = PendingIntentFlags.UpdateCurrent;
        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            flags |= PendingIntentFlags.Immutable;
        }

        return PendingIntent.GetBroadcast(
            context,
            (int)type + 100,
            intent,
            flags)!;
    }

    private sealed class WidgetCompanionBridge
    {
        private readonly Context _context;
        private readonly CompanionUseCase _companion;

        public WidgetCompanionBridge(Context context)
        {
            _context = context.ApplicationContext!;
            var databasePath = Path.Combine(_context.FilesDir!.AbsolutePath!, "niko.db");
            var store = new SqliteStore(databasePath);
            var settings = new UserSettingsStore(databasePath);
            var clock = new WidgetClock();
            _companion = new CompanionUseCase(
                new QuickLogUseCase(store, clock),
                new DashboardUseCase(store, settings, clock),
                new SqliteProcessedMessageStore(databasePath),
                store,
                clock,
                TimeZoneInfo.Local);
        }

        public async Task HandleQuickLogAsync(EventType type)
        {
            var message = new CompanionMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Source = EventSource.Widget,
                MessageType = CompanionMessageType.QuickLog,
                Payload = CompanionMessageSerializer.Serialize(new CompanionQuickLogRequest { EventType = type }),
                SentAtUtc = DateTimeOffset.UtcNow,
            };
            await _companion.HandleAsync(CompanionMessageSerializer.Serialize(message)).ConfigureAwait(false);
        }

        public async Task RefreshAllAsync()
        {
            var manager = AppWidgetManager.GetInstance(_context);
            if (manager is null)
            {
                return;
            }

            var component = new ComponentName(_context, Java.Lang.Class.FromType(typeof(NikoWidgetProvider)));
            var ids = manager.GetAppWidgetIds(component) ?? Array.Empty<int>();
            await RefreshAsync(manager, ids).ConfigureAwait(false);
        }

        public async Task RefreshAsync(AppWidgetManager manager, int[] ids)
        {
            var textContext = await CreateLocalizedContextAsync().ConfigureAwait(false);
            var progress = await RequestAsync<CompanionProgressSummary>(CompanionMessageType.ProgressSummaryRequest).ConfigureAwait(false);
            var streak = await RequestAsync<CompanionStreakSummary>(CompanionMessageType.StreakSummaryRequest).ConfigureAwait(false);
            var sync = await RequestAsync<CompanionSyncStatusSummary>(CompanionMessageType.SyncStatusRequest).ConfigureAwait(false);

            foreach (var id in ids)
            {
                var views = new RemoteViews(_context.PackageName, Resource.Layout.niko_widget);
                views.SetOnClickPendingIntent(Resource.Id.widget_smoked, CreateAction(_context, EventType.Smoked));
                views.SetOnClickPendingIntent(Resource.Id.widget_resisted, CreateAction(_context, EventType.Resisted));
                views.SetOnClickPendingIntent(Resource.Id.widget_craving, CreateAction(_context, EventType.Craving));

                if (progress is not null && streak is not null)
                {
                    views.SetTextViewText(Resource.Id.widget_smoked_today, textContext.GetString(
                        Resource.String.widget_count, progress.SmokedToday));
                    views.SetTextViewText(Resource.Id.widget_resisted_today, textContext.GetString(
                        Resource.String.widget_count, progress.ResistedToday));
                    views.SetTextViewText(Resource.Id.widget_cravings_today, textContext.GetString(
                        Resource.String.widget_count, progress.CravingsToday));
                    views.SetTextViewText(Resource.Id.widget_summary, textContext.GetString(
                        Resource.String.widget_summary_available,
                        streak.CurrentStreakDays,
                        progress.MilestoneProgressPercent));
                    var status = sync?.InSync == true
                        ? textContext.GetString(Resource.String.widget_status_in_sync)
                        : textContext.GetString(Resource.String.widget_status_pending, sync?.PendingCount ?? 0);
                    views.SetTextViewText(Resource.Id.widget_status, status);
                }
                else
                {
                    views.SetTextViewText(Resource.Id.widget_smoked_today, textContext.GetString(Resource.String.widget_status_unavailable));
                    views.SetTextViewText(Resource.Id.widget_resisted_today, textContext.GetString(Resource.String.widget_status_unavailable));
                    views.SetTextViewText(Resource.Id.widget_cravings_today, textContext.GetString(Resource.String.widget_status_unavailable));
                    views.SetTextViewText(Resource.Id.widget_summary, textContext.GetString(Resource.String.widget_status_unavailable));
                    views.SetTextViewText(Resource.Id.widget_status, textContext.GetString(Resource.String.widget_status_offline));
                }

                manager.UpdateAppWidget(id, views);
            }
        }

        private async Task<Context> CreateLocalizedContextAsync()
        {
            var profile = await new UserSettingsStore(
                Path.Combine(_context.FilesDir!.AbsolutePath!, "niko.db"))
                .GetAsync()
                .ConfigureAwait(false);
            var localeCode = NormalizeLocale(profile?.PreferredLocale);
            if (localeCode is null)
            {
                return _context;
            }

            var configuration = new Configuration(_context.Resources!.Configuration);
            configuration.SetLocale(global::Java.Util.Locale.ForLanguageTag(localeCode));
            return _context.CreateConfigurationContext(configuration) ?? _context;
        }

        private static string? NormalizeLocale(string? locale)
            => locale?.ToLowerInvariant() switch
            {
                "en" or "en-us" or "en-gb" => "en",
                "fa" or "fa-ir" => "fa",
                "ar" or "ar-sa" => "ar",
                "zh-hans" or "zh-cn" => "zh-CN",
                _ => null,
            };

        private async Task<T?> RequestAsync<T>(CompanionMessageType type)
        {
            var message = new CompanionMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Source = EventSource.Widget,
                MessageType = type,
                Payload = "{}",
                SentAtUtc = DateTimeOffset.UtcNow,
            };
            var result = await _companion.HandleAsync(CompanionMessageSerializer.Serialize(message)).ConfigureAwait(false);
            return result.Success && result.Data is T value ? value : default;
        }
    }

    private sealed class WidgetClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
