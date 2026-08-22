// ============================================================================
// Niko.Core — ISyncTransport.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد انتقال همگام‌سازی رویدادها به سرویس خارجی. در این مرحله فقط
//           قرارداد تعریف می‌شود و پیاده‌سازی واقعی سرویس هنوز اضافه نمی‌شود.
// وابستگی‌ها و لایه: بخش Abstractions/Sync در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: همگام‌سازی باید idempotent، قابل‌تلاش مجدد و مقاوم در برابر
//           قطع شبکه باشد. سرویس باید بر پایهٔ EventId تکراری را حذف کند.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Abstractions;

/// <summary>
/// قرارداد انتقال رویدادها به سرویس همگام‌سازی. پیاده‌سازی واقعی بعداً افزوده می‌شود.
/// </summary>
public interface ISyncTransport
{
    /// <summary>
    /// ارسال یک دسته رویداد. بر پایهٔ EventId باید idempotent باشد.
    /// </summary>
    Task<SyncResult> PushAsync(
        IReadOnlyList<LogEvent> events,
        CancellationToken ct = default);
}

/// <summary>
/// نتیجهٔ یک ارسال. مشخص می‌کند کدام رویدادها موفق و کدام ناموفق بودند.
/// </summary>
public sealed class SyncResult
{
    public SyncResult(
        IReadOnlyList<string> acceptedEventIds,
        IReadOnlyList<string> failedEventIds)
    {
        AcceptedEventIds = acceptedEventIds;
        FailedEventIds = failedEventIds;
    }

    /// <summary>شناسه رویدادهای پذیرفته‌شده.</summary>
    public IReadOnlyList<string> AcceptedEventIds { get; }

    /// <summary>شناسه رویدادهای ناموفق.</summary>
    public IReadOnlyList<string> FailedEventIds { get; }
}
