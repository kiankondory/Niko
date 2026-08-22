// ============================================================================
// Niko.Core — CravingBattle.cs
// ----------------------------------------------------------------------------
// مسئولیت: موجودیت دامنهٔ «نبرد با هوس». تمام قواعد وضعیت و انتقال‌های مجاز را
//           در Core نگه می‌دارد و وضعیت جاری جلسه را ردیابی می‌کند.
// وابستگی‌ها و لایه: بخش Domain/Craving در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: انتقال‌های نامجاز استثنا پرتاب می‌کنند. حالت‌ها: شروع،
//           انتخاب مداخله، تکمیل، مقاومت و خروج (مصرف) بدون شرم.
// ============================================================================

namespace Niko.Core.Domain.Craving;

/// <summary>
/// وضعیت جلسهٔ نبرد با هوس.
/// </summary>
public enum CravingBattleState
{
    /// <summary>هنوز شروع نشده.</summary>
    Idle = 0,

    /// <summary>جلسه با شدت مشخص شروع شده.</summary>
    Started = 1,

    /// <summary>یک مداخله انتخاب شده است.</summary>
    ActionSelected = 2,

    /// <summary>مداخله/تایمر با موفقیت تکمیل شده.</summary>
    Completed = 3,

    /// <summary>کاربر مقاومت کرده (بدون مصرف).</summary>
    Resisted = 4,

    /// <summary>کاربر خروج زده/مصرف کرده (بدون شرم).</summary>
    ExitedSmoked = 5,
}

/// <summary>
/// جلسهٔ نبرد با هوس و قواعد انتقال وضعیت.
/// </summary>
public sealed class CravingBattle
{
    private CravingBattle(string battleId, CravingIntensity intensity)
    {
        BattleId = battleId;
        Intensity = intensity;
        State = CravingBattleState.Idle;
    }

    /// <summary>شناسهٔ یکتای جلسه.</summary>
    public string BattleId { get; }

    /// <summary>شدت هوس انتخاب‌شده.</summary>
    public CravingIntensity Intensity { get; }

    /// <summary>وضعیت جاری جلسه.</summary>
    public CravingBattleState State { get; private set; }

    /// <summary>مداخلهٔ جاری (در صورت انتخاب).</summary>
    public Intervention? CurrentIntervention { get; private set; }

    /// <summary>شروع یک نبرد جدید با شدت مشخص.</summary>
    public static CravingBattle Start(string battleId, CravingIntensity intensity)
    {
        var battle = new CravingBattle(battleId, intensity);
        battle.State = CravingBattleState.Started;
        return battle;
    }

    /// <summary>انتخاب یک مداخله.</summary>
    public void SelectAction(Intervention intervention)
    {
        RequireState(CravingBattleState.Started, nameof(SelectAction));
        CurrentIntervention = intervention;
        State = CravingBattleState.ActionSelected;
    }

    /// <summary>تکمیل مداخله/تایمر.</summary>
    public void Complete()
    {
        RequireState(CravingBattleState.ActionSelected, nameof(Complete));
        State = CravingBattleState.Completed;
    }

    /// <summary>ثبت مقاومت (بدون مصرف).</summary>
    public void Resist()
    {
        RequireAnyState(new[] { CravingBattleState.Started, CravingBattleState.ActionSelected }, nameof(Resist));
        State = CravingBattleState.Resisted;
    }

    /// <summary>خروج امن (مصرف/انصراف) بدون شرم.</summary>
    public void ExitSmoked()
    {
        RequireAnyState(new[] { CravingBattleState.Started, CravingBattleState.ActionSelected }, nameof(ExitSmoked));
        State = CravingBattleState.ExitedSmoked;
    }

    private void RequireState(CravingBattleState expected, string operation)
    {
        if (State != expected)
        {
            throw new InvalidOperationException(
                $"انتقال نامجاز برای {operation} از وضعیت {State} (نیازمند {expected}).");
        }
    }

    private void RequireAnyState(CravingBattleState[] allowed, string operation)
    {
        if (!allowed.Contains(State))
        {
            throw new InvalidOperationException(
                $"انتقال نامجاز برای {operation} از وضعیت {State}.");
        }
    }
}
