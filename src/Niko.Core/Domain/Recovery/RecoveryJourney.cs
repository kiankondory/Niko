// ============================================================================
// Niko.Core — RecoveryJourney.cs
// ----------------------------------------------------------------------------
// مسئولیت: نگاشت مرحلهٔ تقریبی Recovery به مرحلهٔ انگیزشی و قابل‌نمایش Island.
// وابستگی‌ها و لایه: Domain/Recovery در Core؛ فقط RecoverySnapshot را می‌خواند و
//           هیچ وابستگی به MAUI، ذخیره‌سازی یا شبکه ندارد.
// نکات تغییر و قیود: نگاشت از دادهٔ واقعی ترک مشتق می‌شود؛ ادعای پزشکی، XP، پاداش
//           ساختگی یا دادهٔ خصوصی ایجاد نمی‌کند و بدون دادهٔ کافی غیرفعال است.
// ============================================================================

namespace Niko.Core.Domain.Recovery;

/// <summary>چهار مرحلهٔ تصویریِ مسیر Island.</summary>
public enum RecoveryJourneyStage
{
    Seedling = 1,
    Garden = 2,
    Forest = 3,
    Haven = 4,
}

/// <summary>نمای مشتق‌شدهٔ امن برای تجربهٔ تصویری پیشرفت.</summary>
public sealed record RecoveryJourney(
    RecoveryJourneyStage Stage,
    bool IsAvailable);

/// <summary>سیاست قطعی تبدیل مراحل Recovery به مراحل Island.</summary>
public static class RecoveryJourneyCalculator
{
    public static RecoveryJourney Calculate(RecoverySnapshot recovery)
    {
        if (!recovery.HasSufficientData)
        {
            return new RecoveryJourney(RecoveryJourneyStage.Seedling, false);
        }

        var stage = recovery.Stage switch
        {
            RecoveryStage.Stage0 or RecoveryStage.Stage1 => RecoveryJourneyStage.Seedling,
            RecoveryStage.Stage2 or RecoveryStage.Stage3 => RecoveryJourneyStage.Garden,
            RecoveryStage.Stage4 or RecoveryStage.Stage5 => RecoveryJourneyStage.Forest,
            _ => RecoveryJourneyStage.Haven,
        };

        return new RecoveryJourney(stage, true);
    }
}
