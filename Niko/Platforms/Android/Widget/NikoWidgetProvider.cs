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

        var eventType = (EventType)intent.GetIntExtra(EventTypeExtra, -1);
        if (eventType is not (EventType.Smoked or EventType.Resisted or EventType.Craving))
        {
            return;
        }

        // بازخورد دیداری فوری، پیش از I/O محلی، جلوی لمس پیاپی ناخواسته را می‌گیرد.
        RenderProcessingState(context);

        var pending = GoAsync();
        if (pending is null)
        {
            return;
        }
        _ = ProcessQuickLogAsync(context, eventType, pending);
    }

    private static async Task ProcessQuickLogAsync(Context context, EventType type, PendingResult pending)
    {
        try
        {
            var bridge = new WidgetCompanionBridge(context);
            var saved = await bridge.HandleQuickLogAsync(type).ConfigureAwait(false);
            await bridge.RefreshAllAsync(saved ? type : null).ConfigureAwait(false);
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

    private static void RenderProcessingState(Context context)
    {
        var manager = AppWidgetManager.GetInstance(context);
        if (manager is null)
        {
            return;
        }

        var component = new ComponentName(context, Java.Lang.Class.FromType(typeof(NikoWidgetProvider)));
        var ids = manager.GetAppWidgetIds(component) ?? Array.Empty<int>();
        if (ids.Length == 0)
        {
            return;
        }

        var views = new RemoteViews(context.PackageName, Resource.Layout.niko_widget);
        views.SetTextViewText(Resource.Id.widget_status, context.GetString(Resource.String.widget_action_recording));
        views.SetTextViewText(Resource.Id.widget_summary, context.GetString(Resource.String.widget_action_processing));
        SetActionsEnabled(views, false);
        manager.PartiallyUpdateAppWidget(ids, views);
    }

    private static void SetActionsEnabled(RemoteViews views, bool enabled)
    {
        views.SetBoolean(Resource.Id.widget_smoked, "setEnabled", enabled);
        views.SetBoolean(Resource.Id.widget_resisted, "setEnabled", enabled);
        views.SetBoolean(Resource.Id.widget_craving, "setEnabled", enabled);
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

        public async Task<bool> HandleQuickLogAsync(EventType type)
        {
            var message = new CompanionMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Source = EventSource.Widget,
                MessageType = CompanionMessageType.QuickLog,
                Payload = CompanionMessageSerializer.Serialize(new CompanionQuickLogRequest { EventType = type }),
                SentAtUtc = DateTimeOffset.UtcNow,
            };
            var result = await _companion.HandleAsync(CompanionMessageSerializer.Serialize(message)).ConfigureAwait(false);
            return result.Success;
        }

        public async Task RefreshAllAsync(EventType? feedbackType = null)
        {
            var manager = AppWidgetManager.GetInstance(_context);
            if (manager is null)
            {
                return;
            }

            var component = new ComponentName(_context, Java.Lang.Class.FromType(typeof(NikoWidgetProvider)));
            var ids = manager.GetAppWidgetIds(component) ?? Array.Empty<int>();
            await RefreshAsync(manager, ids, feedbackType).ConfigureAwait(false);
        }

        public async Task RefreshAsync(AppWidgetManager manager, int[] ids, EventType? feedbackType = null)
        {
            var textContext = await CreateLocalizedContextAsync().ConfigureAwait(false);
            var progress = await RequestAsync<CompanionProgressSummary>(CompanionMessageType.ProgressSummaryRequest).ConfigureAwait(false);
            var streak = await RequestAsync<CompanionStreakSummary>(CompanionMessageType.StreakSummaryRequest).ConfigureAwait(false);
            var sync = await RequestAsync<CompanionSyncStatusSummary>(CompanionMessageType.SyncStatusRequest).ConfigureAwait(false);

            foreach (var id in ids)
            {
                var views = new RemoteViews(_context.PackageName, Resource.Layout.niko_widget);
                views.SetTextViewText(Resource.Id.widget_today, textContext.GetString(Resource.String.widget_today));
                views.SetTextViewText(Resource.Id.widget_quick_log, textContext.GetString(Resource.String.widget_quick_log));
                views.SetTextViewText(Resource.Id.widget_smoked, textContext.GetString(Resource.String.widget_smoked));
                views.SetTextViewText(Resource.Id.widget_resisted, textContext.GetString(Resource.String.widget_resisted));
                views.SetTextViewText(Resource.Id.widget_craving, textContext.GetString(Resource.String.widget_craving));
                views.SetOnClickPendingIntent(Resource.Id.widget_smoked, CreateAction(_context, EventType.Smoked));
                views.SetOnClickPendingIntent(Resource.Id.widget_resisted, CreateAction(_context, EventType.Resisted));
                views.SetOnClickPendingIntent(Resource.Id.widget_craving, CreateAction(_context, EventType.Craving));
                SetActionsEnabled(views, true);

                if (progress is not null && streak is not null)
                {
                    views.SetTextViewText(Resource.Id.widget_smoked_count, textContext.GetString(Resource.String.widget_count, progress.SmokedToday));
                    views.SetTextViewText(Resource.Id.widget_smoked_today, textContext.GetString(Resource.String.widget_smoked_today_label));
                    views.SetTextViewText(Resource.Id.widget_resisted_count, textContext.GetString(Resource.String.widget_count, progress.ResistedToday));
                    views.SetTextViewText(Resource.Id.widget_resisted_today, textContext.GetString(Resource.String.widget_resisted_today_label));
                    views.SetTextViewText(Resource.Id.widget_saved_today, textContext.GetString(
                        Resource.String.widget_saved_today,
                        new Java.Lang.String(FormatDailySavings(progress, textContext))));
                    views.SetTextViewText(Resource.Id.widget_value_per_cigarette, textContext.GetString(
                        Resource.String.widget_value_per_cigarette,
                        new Java.Lang.String(FormatPerResistedCigarette(progress, textContext))));
                    views.SetTextViewText(Resource.Id.widget_summary, textContext.GetString(
                        Resource.String.widget_summary_available,
                        streak.CurrentStreakDays,
                        progress.MilestoneProgressPercent));
                    var status = feedbackType is { } savedType
                        ? textContext.GetString(Resource.String.widget_action_saved, new Java.Lang.String(GetActionLabel(textContext, savedType)))
                        : sync?.InSync == true
                        ? textContext.GetString(Resource.String.widget_status_in_sync)
                        : textContext.GetString(Resource.String.widget_status_pending, sync?.PendingCount ?? 0);
                    views.SetTextViewText(Resource.Id.widget_status, status);
                }
                else
                {
                    views.SetTextViewText(Resource.Id.widget_smoked_count, textContext.GetString(Resource.String.widget_unavailable_count));
                    views.SetTextViewText(Resource.Id.widget_smoked_today, textContext.GetString(Resource.String.widget_status_unavailable));
                    views.SetTextViewText(Resource.Id.widget_resisted_count, textContext.GetString(Resource.String.widget_unavailable_count));
                    views.SetTextViewText(Resource.Id.widget_resisted_today, textContext.GetString(Resource.String.widget_status_unavailable));
                    views.SetTextViewText(Resource.Id.widget_saved_today, textContext.GetString(
                        Resource.String.widget_saved_today,
                        new Java.Lang.String(textContext.GetString(Resource.String.widget_unavailable_count))));
                    views.SetTextViewText(Resource.Id.widget_value_per_cigarette, textContext.GetString(
                        Resource.String.widget_value_per_cigarette,
                        new Java.Lang.String(textContext.GetString(Resource.String.widget_unavailable_count))));
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
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                return null;
            }

            return locale.Equals("zh-Hans", StringComparison.OrdinalIgnoreCase)
                ? "zh-CN"
                : locale.Equals("zh-Hant", StringComparison.OrdinalIgnoreCase)
                    ? "zh-TW"
                    : locale;
        }

        private static string FormatDailySavings(CompanionProgressSummary progress, Context textContext)
        {
            if (progress.DailySavedAmount is not { } amount || string.IsNullOrWhiteSpace(progress.DailySavingsCurrencyCode))
            {
                return textContext.GetString(Resource.String.widget_unavailable_count);
            }

            var languageTag = GetLanguageTag(textContext);
            var culture = TryGetCulture(languageTag);
            return string.Concat(amount.ToString("N2", culture), " ", progress.DailySavingsCurrencyCode);
        }

        private static string FormatPerResistedCigarette(CompanionProgressSummary progress, Context textContext)
        {
            if (progress.AmountPerResistedCigarette is not { } amount ||
                string.IsNullOrWhiteSpace(progress.DailySavingsCurrencyCode))
            {
                return textContext.GetString(Resource.String.widget_unavailable_count);
            }

            var culture = TryGetCulture(GetLanguageTag(textContext));
            return string.Concat(amount.ToString("N2", culture), " ", progress.DailySavingsCurrencyCode);
        }

        private static System.Globalization.CultureInfo TryGetCulture(string? languageTag)
        {
            try
            {
                return string.IsNullOrWhiteSpace(languageTag)
                    ? System.Globalization.CultureInfo.InvariantCulture
                    : System.Globalization.CultureInfo.GetCultureInfo(languageTag);
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                return System.Globalization.CultureInfo.InvariantCulture;
            }
        }

        private static string? GetLanguageTag(Context textContext)
        {
            var configuration = textContext.Resources?.Configuration;
            if (configuration is null)
            {
                return null;
            }

            return OperatingSystem.IsAndroidVersionAtLeast(24)
                ? configuration.Locales?.Get(0)?.ToLanguageTag()
                : configuration.Locale?.ToLanguageTag();
        }

        private static string GetActionLabel(Context textContext, EventType type)
            => type switch
            {
                EventType.Smoked => textContext.GetString(Resource.String.widget_smoked),
                EventType.Resisted => textContext.GetString(Resource.String.widget_resisted),
                EventType.Craving => textContext.GetString(Resource.String.widget_craving),
                _ => textContext.GetString(Resource.String.widget_quick_log),
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
