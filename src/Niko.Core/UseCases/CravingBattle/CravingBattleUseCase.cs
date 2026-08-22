// ============================================================================
// Niko.Core — CravingBattleUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: مورد کاربرد «نبرد با هوس». جلسه را شروع، مداخله را انتخاب، تکمیل،
//           مقاومت یا خروج امن را اعمال می‌کند و رویدادهای مربوطه را از مسیر
//           ذخیره‌ساز محلی (outbox) پایدار می‌سازد تا آفلاین و قابل بازیابی باشند.
// وابستگی‌ها و لایه: UseCases/CravingBattle → Abstractions (ILocalStore, IClock)
//           + Domain/Craving + Events.
// نکات تغییر و قیود: تمام قواعد انتقال در موجودیت دامنه (CravingBattle) است.
//           رویدادهای outcome به‌صورت Pending ذخیره می‌شوند تا صف همگام‌سازی آن‌ها
//           را بپذیرد. مقاومت/مصرف به‌عنوان رویداد Resisted/Smoked نیز ثبت می‌شود
//           تا داشبورد آن‌ها را بشمارد.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Craving;
using Niko.Core.Events;

namespace Niko.Core.UseCases.CravingBattle;

using Battle = Niko.Core.Domain.Craving.CravingBattle;

/// <summary>
/// مورد کاربرد نبرد با هوس.
/// </summary>
public sealed class CravingBattleUseCase
{
    private readonly ILocalStore _store;
    private readonly IClock _clock;
    private Battle? _activeBattle;

    public CravingBattleUseCase(ILocalStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    /// <summary>شروع نبرد با شدت مشخص.</summary>
    public async Task<CravingBattleResult> StartAsync(
        CravingIntensity intensity,
        CancellationToken ct = default)
    {
        var battle = Battle.Start(Guid.NewGuid().ToString("N"), intensity);
        _activeBattle = battle;

        await PersistAsync(battle, "started",
            new Dictionary<string, string>
            {
                ["intensity"] = intensity.ToString(),
            }, ct).ConfigureAwait(false);

        // ثبت خود هوس تا در داشبورد شمارش شود.
        await _store.SaveEventAsync(new LogEvent(
            Guid.NewGuid().ToString("N"),
            _clock.UtcNow,
            EventSource.Mobile,
            EventType.Craving,
            SyncStatus.Pending), ct).ConfigureAwait(false);

        return ToResult(battle);
    }

    /// <summary>انتخاب مداخلهٔ جاری.</summary>
    public async Task<CravingBattleResult> SelectActionAsync(
        Intervention intervention,
        CancellationToken ct = default)
    {
        var battle = RequireActive();
        battle.SelectAction(intervention);

        await PersistAsync(battle, "action_selected",
            new Dictionary<string, string>
            {
                ["action"] = intervention.ToString(),
            }, ct).ConfigureAwait(false);

        return ToResult(battle);
    }

    /// <summary>تکمیل مداخله/تایمر.</summary>
    public async Task<CravingBattleResult> CompleteAsync(CancellationToken ct = default)
    {
        var battle = RequireActive();
        battle.Complete();

        await PersistAsync(battle, "completed", null, ct).ConfigureAwait(false);

        return ToResult(battle);
    }

    /// <summary>ثبت مقاومت (بدون مصرف).</summary>
    public async Task<CravingBattleResult> ResistAsync(CancellationToken ct = default)
    {
        var battle = RequireActive();
        battle.Resist();

        await PersistAsync(battle, "resisted", null, ct).ConfigureAwait(false);

        // ثبت مقاومت برای شمارش در داشبورد.
        await _store.SaveEventAsync(new LogEvent(
            Guid.NewGuid().ToString("N"),
            _clock.UtcNow,
            EventSource.Mobile,
            EventType.Resisted,
            SyncStatus.Pending), ct).ConfigureAwait(false);

        return ToResult(battle);
    }

    /// <summary>خروج امن (مصرف/انصراف) بدون شرم.</summary>
    public async Task<CravingBattleResult> ExitSmokedAsync(CancellationToken ct = default)
    {
        var battle = RequireActive();
        battle.ExitSmoked();

        await PersistAsync(battle, "exited_smoked", null, ct).ConfigureAwait(false);

        // ثبت مصرف برای شمارش در داشبورد.
        await _store.SaveEventAsync(new LogEvent(
            Guid.NewGuid().ToString("N"),
            _clock.UtcNow,
            EventSource.Mobile,
            EventType.Smoked,
            SyncStatus.Pending), ct).ConfigureAwait(false);

        return ToResult(battle);
    }

    private Battle RequireActive()
    {
        if (_activeBattle is null)
        {
            throw new InvalidOperationException("هیچ نبرد فعالی وجود ندارد.");
        }

        return _activeBattle;
    }

    private async Task PersistAsync(
        Battle battle,
        string stage,
        IReadOnlyDictionary<string, string>? extra,
        CancellationToken ct)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["stage"] = stage,
            ["battle_id"] = battle.BattleId,
        };

        if (extra is not null)
        {
            foreach (var (key, value) in extra)
            {
                metadata[key] = value;
            }
        }

        await _store.SaveEventAsync(new LogEvent(
            Guid.NewGuid().ToString("N"),
            _clock.UtcNow,
            EventSource.Mobile,
            EventType.CravingAction,
            SyncStatus.Pending,
            metadata), ct).ConfigureAwait(false);
    }

    private static CravingBattleResult ToResult(Battle battle)
        => new(
            battle.BattleId,
            battle.State,
            battle.Intensity,
            battle.CurrentIntervention);
}
