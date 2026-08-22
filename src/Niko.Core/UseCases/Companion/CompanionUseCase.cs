// ============================================================================
// Niko.Core — CompanionUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: نقطهٔ ورود مشترک هسته برای پیام‌های ابزارک/ساعت. پیام را اعتبارسنجی،
//           نسخهٔ قرارداد را بررسی، تکراری را حذف و سپس به مورد کاربردهای مشترک
//           (QuickLog/Dashboard) هدایت می‌کند. هیچ منطق دامنهٔ موازی ندارد.
// وابستگی‌ها و لایه: UseCases/Companion → Abstractions (ILocalStore,
//           IProcessedMessageStore) + UseCases (QuickLog, Dashboard) + Domain/CompanionContracts.
// نکات تغییر و قیود: شکست به‌صورت امن (بدون استثنا) بازگردانده می‌شود. MessageId
//           کلید idempotency است.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Events;
using Niko.Core.UseCases.Dashboard;
using Niko.Core.UseCases.QuickLog;

namespace Niko.Core.UseCases.Companion;

/// <summary>
/// مورد کاربرد پردازش پیام ابزارک/ساعت.
/// </summary>
public sealed class CompanionUseCase : ICompanionAdapter
{
    private static readonly HashSet<EventType> _allowedQuickLogTypes = new()
    {
        EventType.Smoked,
        EventType.Resisted,
        EventType.Craving,
    };

    private readonly QuickLogUseCase _quickLog;
    private readonly DashboardUseCase _dashboard;
    private readonly IProcessedMessageStore _processedStore;
    private readonly ILocalStore _store;
    private readonly IClock _clock;
    private readonly TimeZoneInfo _localTimeZone;

    public CompanionUseCase(
        QuickLogUseCase quickLog,
        DashboardUseCase dashboard,
        IProcessedMessageStore processedStore,
        ILocalStore store,
        IClock clock,
        TimeZoneInfo? localTimeZone = null)
    {
        _quickLog = quickLog;
        _dashboard = dashboard;
        _processedStore = processedStore;
        _store = store;
        _clock = clock;
        _localTimeZone = localTimeZone ?? TimeZoneInfo.Local;
    }

    public async Task<CompanionResult<object>> HandleAsync(
        string serializedMessage,
        CancellationToken ct = default)
    {
        var message = CompanionMessageSerializer.DeserializeMessage(serializedMessage);
        if (message is null)
        {
            return CompanionResult<object>.Fail(CompanionErrorCode.MalformedPayload);
        }

        if (!CompanionMessageSerializer.IsVersionSupported(message.ContractVersion))
        {
            return CompanionResult<object>.Fail(CompanionErrorCode.UnsupportedVersion);
        }

        if (!IsValidSource(message.Source))
        {
            return CompanionResult<object>.Fail(CompanionErrorCode.InvalidSource);
        }

        if (!await _processedStore.TryMarkProcessedAsync(message.MessageId, ct).ConfigureAwait(false))
        {
            return CompanionResult<object>.Fail(CompanionErrorCode.DuplicateEvent);
        }

        return await DispatchAsync(message, ct).ConfigureAwait(false);
    }

    private async Task<CompanionResult<object>> DispatchAsync(
        CompanionMessage message,
        CancellationToken ct)
    {
        switch (message.MessageType)
        {
            case CompanionMessageType.QuickLog:
                return await HandleQuickLogAsync(message, ct).ConfigureAwait(false);

            case CompanionMessageType.ProgressSummaryRequest:
                return await HandleProgressSummaryAsync(ct).ConfigureAwait(false);

            case CompanionMessageType.StreakSummaryRequest:
                return await HandleStreakSummaryAsync(ct).ConfigureAwait(false);

            case CompanionMessageType.SyncStatusRequest:
                return await HandleSyncStatusAsync(ct).ConfigureAwait(false);

            default:
                return CompanionResult<object>.Fail(CompanionErrorCode.UnknownMessage);
        }
    }

    private async Task<CompanionResult<object>> HandleQuickLogAsync(
        CompanionMessage message,
        CancellationToken ct)
    {
        var request = CompanionMessageSerializer.DeserializePayload<CompanionQuickLogRequest>(message.Payload);
        if (request is null)
        {
            return CompanionResult<object>.Fail(CompanionErrorCode.MalformedPayload);
        }

        if (!_allowedQuickLogTypes.Contains(request.EventType))
        {
            return CompanionResult<object>.Fail(CompanionErrorCode.ValidationFailed);
        }

        var result = await _quickLog.ExecuteAsync(new QuickLogRequest(
            request.EventType,
            request.Intensity,
            request.Context,
            message.Source,
            request.OccurredAtUtc,
            request.EventId), ct).ConfigureAwait(false);

        var response = new CompanionQuickLogResponse(true, result.EventId, result.SyncStatus);
        return CompanionResult<object>.Ok(response);
    }

    private async Task<CompanionResult<object>> HandleProgressSummaryAsync(CancellationToken ct)
    {
        var dashboard = await _dashboard.ExecuteAsync(ct).ConfigureAwait(false);
        var dailySummary = CompanionDailySummaryCalculator.Calculate(
            await LoadAllEventsAsync(ct).ConfigureAwait(false),
            _clock.UtcNow,
            _localTimeZone);
        var summary = new CompanionProgressSummary(
            dashboard.Snapshot.TotalSmoked,
            dashboard.Snapshot.TotalResisted,
            dashboard.Snapshot.TotalCravings,
            dashboard.Snapshot.MilestoneProgressPercent,
            dashboard.Snapshot.ApproximateSavings is not null)
        {
            SmokedToday = dailySummary.SmokedToday,
            ResistedToday = dailySummary.ResistedToday,
            CravingsToday = dailySummary.CravingsToday,
        };

        return CompanionResult<object>.Ok(summary);
    }

    private async Task<IReadOnlyList<LogEvent>> LoadAllEventsAsync(CancellationToken ct)
    {
        var events = new List<LogEvent>();
        var offset = 0;

        while (true)
        {
            var batch = await _store.GetEventsAsync(offset, 500, ct).ConfigureAwait(false);
            events.AddRange(batch);
            if (batch.Count < 500)
            {
                return events;
            }

            offset += batch.Count;
        }
    }

    private async Task<CompanionResult<object>> HandleStreakSummaryAsync(CancellationToken ct)
    {
        var dashboard = await _dashboard.ExecuteAsync(ct).ConfigureAwait(false);
        var summary = new CompanionStreakSummary(
            dashboard.Snapshot.CurrentStreakDays,
            dashboard.Snapshot.CurrentMilestoneDays,
            dashboard.Snapshot.NextMilestoneDays);

        return CompanionResult<object>.Ok(summary);
    }

    private async Task<CompanionResult<object>> HandleSyncStatusAsync(CancellationToken ct)
    {
        var pending = await _store.GetPendingEventsAsync(limit: 1, ct: ct).ConfigureAwait(false);
        var summary = new CompanionSyncStatusSummary(
            pending.Count,
            InSync: pending.Count == 0);

        return CompanionResult<object>.Ok(summary);
    }

    private static bool IsValidSource(EventSource source)
        => source is EventSource.Mobile or EventSource.Wearable or EventSource.Widget;
}
