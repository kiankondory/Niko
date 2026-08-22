// ============================================================================
// Niko.Core — MilestoneCalculator.cs
// ----------------------------------------------------------------------------
// مسئولیت: تعیین وضعیت هر میل‌استون (تکمیل‌شده/فعلی/آینده) بر پایهٔ روزهای استریک.
//           محاسبهٔ خالص و بدون I/O است.
// وابستگی‌ها و لایه: بخش Domain در Core؛ از آستانه‌های ProgressCalculator استفاده می‌کند.
// نکات تغییر و قیود: میل‌استون «فعلی» اولین آستانه‌ای است که هنوز محقق نشده؛
//           آستانه‌های قبلی «تکمیل‌شده» و بعدی «آینده» محسوب می‌شوند.
// ============================================================================

namespace Niko.Core.Domain;

/// <summary>
/// محاسبه‌گر وضعیت میل‌استون‌ها.
/// </summary>
public static class MilestoneCalculator
{
    /// <summary>
    /// فهرست همهٔ میل‌استون‌ها به‌همراه وضعیت‌شان بر پایهٔ روزهای استریک.
    /// </summary>
    public static IReadOnlyList<MilestoneInfo> GetMilestones(int streakDays)
    {
        var result = new List<MilestoneInfo>(ProgressCalculator.MilestoneThresholds.Length);
        var currentAssigned = false;

        foreach (var threshold in ProgressCalculator.MilestoneThresholds)
        {
            MilestoneStatus status;

            if (threshold <= streakDays)
            {
                status = MilestoneStatus.Completed;
            }
            else if (!currentAssigned)
            {
                status = MilestoneStatus.Current;
                currentAssigned = true;
            }
            else
            {
                status = MilestoneStatus.Upcoming;
            }

            result.Add(new MilestoneInfo(threshold, status));
        }

        return result;
    }
}
