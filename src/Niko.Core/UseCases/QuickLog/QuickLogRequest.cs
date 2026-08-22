// ============================================================================
// Niko.Core — QuickLogRequest.cs
// ----------------------------------------------------------------------------
// مسئولیت: دادهٔ ورودی موردنیاز برای ثبت سریع یک رویداد (دود، مقاومت یا هوس).
// وابستگی‌ها و لایه: بخش UseCases/QuickLog در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: فقط دادهٔ حداقلی پذیرفته می‌شود؛ شدت و زمینه اختیاری‌اند.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.UseCases.QuickLog;

/// <summary>
/// درخواست ثبت سریع یک رویداد.
/// </summary>
public sealed record QuickLogRequest(
    EventType Type,
    int? Intensity = null,
    string? Context = null,
    EventSource Source = EventSource.Mobile,
    DateTimeOffset? OccurredAtUtc = null,
    string? EventId = null);
