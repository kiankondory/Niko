// ============================================================================
// Niko.Core — Intervention.cs
// ----------------------------------------------------------------------------
// مسئولیت: تعریف مداخلات کوتاه و امن «نبرد با هوس» (تنفس عمیق، تأخیر، آب، حرکت،
//           تماس/پشتیبانی) به‌همراه مدت زمان پیشنهادی هرکدام. این مداخلات غیرپزشکی
//           و حمایتی‌اند.
// وابستگی‌ها و لایه: بخش Domain/Craving در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: مدت‌ها به‌صورت ثانیه ثابت‌اند و کوتاه‌اند. کلیدهای متنی
//           در منابع locale تعریف می‌شوند.
// ============================================================================

namespace Niko.Core.Domain.Craving;

/// <summary>
/// نوع مداخلهٔ نبرد با هوس.
/// </summary>
public enum Intervention
{
    /// <summary>تنفس عمیق.</summary>
    DeepBreathing = 0,

    /// <summary>تأخیر/تایمر.</summary>
    Delay = 1,

    /// <summary>نوشیدن آب.</summary>
    DrinkWater = 2,

    /// <summary>حرکت/فعالیت سبک.</summary>
    Movement = 3,

    /// <summary>تماس با پشتیبانی/یک فرد قابل‌اعتماد.</summary>
    SupportContact = 4,
}

/// <summary>
/// فهرست مداخله‌ها با مدت زمان پیشنهادی (ثانیه).
/// </summary>
public static class InterventionCatalog
{
    /// <summary>مدت زمان پیشنهادی هر مداخله بر حسب ثانیه.</summary>
    public static readonly IReadOnlyDictionary<Intervention, int> DurationSeconds =
        new Dictionary<Intervention, int>
        {
            [Intervention.DeepBreathing] = 60,
            [Intervention.Delay] = 120,
            [Intervention.DrinkWater] = 90,
            [Intervention.Movement] = 120,
            [Intervention.SupportContact] = 180,
        };

    /// <summary>مدت زمان پیشنهادی یک مداخله؛ اگر ناشناخته بود مقدار پیش‌فرض.</summary>
    public static int GetDurationSeconds(Intervention intervention)
        => DurationSeconds.TryGetValue(intervention, out var seconds) ? seconds : 60;
}
