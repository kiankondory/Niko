// ============================================================================
// Niko.Core — CompanionResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: پاکت نتیجهٔ پردازش پیام ابزارک/ساعت. موفقیت، کد خطا و دادهٔ اختیاری
//           را به‌صورت ساختاری برمی‌گرداند؛ هیچ استثنایی به بیرون نمی‌رود.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core.
// نکات تغییر و قیود: شکست به‌صورت امن بازگردانده می‌شود؛ پیام خطا در منابع locale است.
// ============================================================================

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// پاکت نتیجهٔ پردازش پیام ابزارک/ساعت.
/// </summary>
public sealed record CompanionResult<T>(
    bool Success,
    CompanionErrorCode ErrorCode,
    T? Data = default)
{
    /// <summary>نتیجهٔ موفق.</summary>
    public static CompanionResult<T> Ok(T data)
        => new(true, CompanionErrorCode.None, data);

    /// <summary>نتیجهٔ ناموفق با کد خطا.</summary>
    public static CompanionResult<T> Fail(CompanionErrorCode code)
        => new(false, code);
}
