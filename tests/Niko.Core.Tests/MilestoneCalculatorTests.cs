// ============================================================================
// Niko.Core.Tests — MilestoneCalculatorTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های وضعیت میل‌استون‌ها: تکمیل‌شده، فعلی، آینده، حالت خالی و حالت‌های
//           مرزی (دقیقاً روی آستانه، فراتر از همهٔ آستانه‌ها).
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و MilestoneCalculator را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و به شبکه/زمان واقعی وابسته نیستند.
// ============================================================================

using Niko.Core.Domain;

namespace Niko.Core.Tests;

public class MilestoneCalculatorTests
{
    [Fact]
    public void ZeroStreak_FirstMilestoneIsCurrent_RestUpcoming()
    {
        var milestones = MilestoneCalculator.GetMilestones(0);

        Assert.Equal(MilestoneStatus.Current, milestones[0].Status);
        Assert.Equal(1, milestones[0].ThresholdDays);
        Assert.All(milestones.Skip(1), m => Assert.Equal(MilestoneStatus.Upcoming, m.Status));
        Assert.DoesNotContain(milestones, m => m.Status == MilestoneStatus.Completed);
    }

    [Fact]
    public void StreakExactlyOnThreshold_ThatThresholdIsCompleted_NextIsCurrent()
    {
        // استریک ۷: آستانه‌های ۱،۳،۷ تکمیل‌شده؛ ۱۴ فعلی.
        var milestones = MilestoneCalculator.GetMilestones(7);

        Assert.Equal(new[] { 1, 3, 7 },
            milestones.Where(m => m.Status == MilestoneStatus.Completed)
                      .Select(m => m.ThresholdDays));
        Assert.Equal(14, milestones.First(m => m.Status == MilestoneStatus.Current).ThresholdDays);
    }

    [Fact]
    public void StreakBetweenThresholds_CompletedUpToFloor_CurrentIsCeiling()
    {
        // استریک ۱۰: ۱،۳،۷ تکمیل‌شده؛ ۱۴ فعلی؛ بقیه آینده.
        var milestones = MilestoneCalculator.GetMilestones(10);

        Assert.Equal(new[] { 1, 3, 7 },
            milestones.Where(m => m.Status == MilestoneStatus.Completed)
                      .Select(m => m.ThresholdDays));
        Assert.Equal(14, milestones.First(m => m.Status == MilestoneStatus.Current).ThresholdDays);
        Assert.Equal(new[] { 30, 60, 90, 180, 365 },
            milestones.Where(m => m.Status == MilestoneStatus.Upcoming)
                      .Select(m => m.ThresholdDays));
    }

    [Fact]
    public void StreakExactlyOne_DayOneMilestoneCompleted()
    {
        var milestones = MilestoneCalculator.GetMilestones(1);

        Assert.Equal(MilestoneStatus.Completed, milestones[0].Status);
        Assert.Equal(MilestoneStatus.Current, milestones[1].Status);
    }

    [Fact]
    public void StreakBeyondAllThresholds_AllCompleted_NoCurrentOrUpcoming()
    {
        // استریک ۴۰۰: همهٔ آستانه‌ها تا ۳۶۵ تکمیل‌شده.
        var milestones = MilestoneCalculator.GetMilestones(400);

        Assert.All(milestones, m => Assert.Equal(MilestoneStatus.Completed, m.Status));
        Assert.Equal(ProgressCalculator.MilestoneThresholds.Length, milestones.Count);
    }

    [Fact]
    public void Thresholds_AreExactlySpecifiedSet()
    {
        Assert.Equal(new[] { 1, 3, 7, 14, 30, 60, 90, 180, 365 },
            ProgressCalculator.MilestoneThresholds);
    }
}
