// ============================================================================
// Niko.Core — RecoveryStage.cs
// ----------------------------------------------------------------------------
// مسئولیت: تعریف مراحل بهبود بدن بر پایهٔ زمان بدون مصرف. این مراحل تقریبی،
//           غیرپزشکی و فقط برای آگاهی هستند؛ هیچ ادعای قطعی پزشکی ندارند.
// وابستگی‌ها و لایه: بخش Domain/Recovery در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: کلیدهای متنی (عنوان/توضیح) در منابع locale تعریف می‌شوند؛
//           Core فقط شناسهٔ مرحله را برمی‌گرداند. مرزها به‌صورت روز پایدارند.
// ============================================================================

namespace Niko.Core.Domain.Recovery;

/// <summary>
/// مرحلهٔ تقریبی بهبود بر پایهٔ روزهای بدون مصرف.
/// </summary>
public enum RecoveryStage
{
    /// <summary>شروع/کمتر از ۱ روز.</summary>
    Stage0 = 0,

    /// <summary>۱ تا ۳ روز.</summary>
    Stage1 = 1,

    /// <summary>۳ تا ۷ روز.</summary>
    Stage2 = 2,

    /// <summary>۷ تا ۱۴ روز.</summary>
    Stage3 = 3,

    /// <summary>۱۴ تا ۳۰ روز.</summary>
    Stage4 = 4,

    /// <summary>۳۰ تا ۹۰ روز.</summary>
    Stage5 = 5,

    /// <summary>۹۰ تا ۱۸۰ روز.</summary>
    Stage6 = 6,

    /// <summary>بیش از ۱۸۰ روز.</summary>
    Stage7 = 7,
}

/// <summary>
/// مشخصات یک مرحله: بازهٔ روز، شناسهٔ مرحله و شناسهٔ مرحلهٔ بعدی.
/// </summary>
public sealed record RecoveryStageInfo(
    RecoveryStage Stage,
    int FromDays,
    int? ToDays,
    string KeyPrefix)
{
    /// <summary>شناسهٔ مرحلهٔ بعدی؛ برای آخرین مرحله null.</summary>
    public RecoveryStage? NextStage =>
        Stage == RecoveryStage.Stage7 ? null : Stage + 1;
}
