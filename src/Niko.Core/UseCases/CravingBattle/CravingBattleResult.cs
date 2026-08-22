// ============================================================================
// Niko.Core — CravingBattleResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ عملیات‌های نبرد با هوس؛ شامل شناسه، وضعیت، مداخله و شدت جاری.
// وابستگی‌ها و لایه: بخش UseCases/CravingBattle در Core.
// نکات تغییر و قیود: فقط دادهٔ وضعیت؛ هیچ منطقی ندارد.
// ============================================================================

using Niko.Core.Domain.Craving;

namespace Niko.Core.UseCases.CravingBattle;

/// <summary>
/// نتیجهٔ یک عملیات در نبرد با هوس.
/// </summary>
public sealed record CravingBattleResult(
    string BattleId,
    CravingBattleState State,
    CravingIntensity Intensity,
    Intervention? CurrentIntervention = null);
