// ============================================================================
// Niko.Core.Tests — CravingBattleTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های قواعد وضعیت نبرد با هوس: شروع، انتخاب مداخله، تکمیل، مقاومت،
//           خروج و انتقال‌های نامجاز.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و موجودیت دامنه را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و به شبکه/زمان واقعی وابسته نیستند.
// ============================================================================

using Niko.Core.Domain.Craving;

namespace Niko.Core.Tests;

public class CravingBattleTests
{
    private static CravingBattle Create()
        => CravingBattle.Start("b1", CravingIntensity.Moderate);

    [Fact]
    public void Start_SetsStartedState_WithIntensity()
    {
        var battle = CravingBattle.Start("b1", CravingIntensity.Intense);

        Assert.Equal(CravingBattleState.Started, battle.State);
        Assert.Equal(CravingIntensity.Intense, battle.Intensity);
        Assert.Equal("b1", battle.BattleId);
    }

    [Theory]
    [InlineData(CravingIntensity.Mild)]
    [InlineData(CravingIntensity.Moderate)]
    [InlineData(CravingIntensity.Intense)]
    public void Start_WithAnyIntensity_IsStarted(CravingIntensity intensity)
    {
        var battle = CravingBattle.Start("b", intensity);
        Assert.Equal(CravingBattleState.Started, battle.State);
    }

    [Fact]
    public void SelectAction_FromStarted_MovesToActionSelected()
    {
        var battle = Create();
        battle.SelectAction(Intervention.DeepBreathing);

        Assert.Equal(CravingBattleState.ActionSelected, battle.State);
        Assert.Equal(Intervention.DeepBreathing, battle.CurrentIntervention);
    }

    [Fact]
    public void Complete_FromActionSelected_MovesToCompleted()
    {
        var battle = Create();
        battle.SelectAction(Intervention.Delay);
        battle.Complete();

        Assert.Equal(CravingBattleState.Completed, battle.State);
    }

    [Fact]
    public void Resist_FromStarted_And_FromActionSelected_MovesToResisted()
    {
        var fromStarted = Create();
        fromStarted.Resist();
        Assert.Equal(CravingBattleState.Resisted, fromStarted.State);

        var fromAction = Create();
        fromAction.SelectAction(Intervention.Movement);
        fromAction.Resist();
        Assert.Equal(CravingBattleState.Resisted, fromAction.State);
    }

    [Fact]
    public void ExitSmoked_FromStarted_And_FromActionSelected_MovesToExited()
    {
        var fromStarted = Create();
        fromStarted.ExitSmoked();
        Assert.Equal(CravingBattleState.ExitedSmoked, fromStarted.State);

        var fromAction = Create();
        fromAction.SelectAction(Intervention.SupportContact);
        fromAction.ExitSmoked();
        Assert.Equal(CravingBattleState.ExitedSmoked, fromAction.State);
    }

    [Fact]
    public void SelectAction_Twice_Throws()
    {
        var battle = Create();
        battle.SelectAction(Intervention.Delay);

        Assert.Throws<InvalidOperationException>(() => battle.SelectAction(Intervention.Movement));
    }

    [Fact]
    public void Complete_FromStarted_WithoutAction_Throws()
    {
        var battle = Create();

        Assert.Throws<InvalidOperationException>(() => battle.Complete());
    }

    [Fact]
    public void Complete_FromResisted_Throws()
    {
        var battle = Create();
        battle.Resist();

        Assert.Throws<InvalidOperationException>(() => battle.Complete());
    }

    [Fact]
    public void Resist_AfterCompleted_Throws()
    {
        var battle = Create();
        battle.SelectAction(Intervention.Delay);
        battle.Complete();

        Assert.Throws<InvalidOperationException>(() => battle.Resist());
    }

    [Fact]
    public void ExitSmoked_AfterCompleted_Throws()
    {
        var battle = Create();
        battle.SelectAction(Intervention.Delay);
        battle.Complete();

        Assert.Throws<InvalidOperationException>(() => battle.ExitSmoked());
    }

    [Fact]
    public void DurationSeconds_ReturnsPositiveValue()
    {
        foreach (Intervention intervention in Enum.GetValues<Intervention>())
        {
            Assert.True(InterventionCatalog.GetDurationSeconds(intervention) > 0);
        }
    }
}
