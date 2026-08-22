// ============================================================================
// Niko.Core — RecoverySnapshot.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ محاسبهٔ بهبود: مرحلهٔ فعلی، درصد پیشرفت به سمت مرحلهٔ بعدی،
//           زمان بدون مصرف و آیا دادهٔ کافی موجود است. فقط دادهٔ مشتق‌شده است.
// وابستگی‌ها و لایه: بخش Domain/Recovery در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: زمان بدون مصرف و درصد از محاسبهٔ خالص می‌آیند. بدون دادهٔ
//           کافی، مرحلهٔ صفر و بدون زمان برمی‌گردد.
// ============================================================================

namespace Niko.Core.Domain.Recovery;

/// <summary>
/// نمای محاسبه‌شدهٔ بهبود بدن.
/// </summary>
public sealed record RecoverySnapshot
{
    /// <summary>مرحلهٔ فعلی.</summary>
    public RecoveryStage Stage { get; init; }

    /// <summary>درصد پیشرفت به سمت مرحلهٔ بعدی (۰ تا ۱۰۰).</summary>
    public double ProgressPercent { get; init; }

    /// <summary>زمان بدون مصرف (بر حسب زمان).</summary>
    public TimeSpan SmokeFreeTime { get; init; }

    /// <summary>آیا دادهٔ کافی برای محاسبهٔ معنی‌دار وجود دارد؟</summary>
    public bool HasSufficientData { get; init; }
}
