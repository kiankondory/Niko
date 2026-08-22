// ============================================================================
// Niko.Core — LogEvent.cs
// ----------------------------------------------------------------------------
// مسئولیت: مدل مرکزی رویداد ثبت‌شده. هر رویداد دارای شناسهٔ یکتا، زمان (UTC)،
//           منبع، نوع، ابردادهٔ محدود و وضعیت همگام‌سازی است.
// وابستگی‌ها و لایه: بخش Events در Core؛ مدل سریال‌پذیر برای صف همگام‌سازی.
// نکات تغییر و قیود: قرارداد رویداد باید نسخه‌بندی و سازگار با عقب‌ماندگی باشد.
//           تغییرات معنایی نیازمند مسیر مهاجرت و ثبت در DECISIONS.md است.
//           EventId به‌عنوان کلید یکتایی و کلید idempotency همگام‌سازی است.
// ============================================================================

using Niko.Core.Abstractions;

namespace Niko.Core.Events;

/// <summary>
/// یک رویداد ثبت‌شده. به‌صورت تغییرناپذیر طراحی شده و برای ذخیره/همگام‌سازی
/// با همان فیلدهای پایدار سریال می‌شود.
/// </summary>
public sealed class LogEvent
{
    public LogEvent(
        string eventId,
        DateTimeOffset occurredAtUtc,
        EventSource source,
        EventType type,
        SyncStatus syncStatus,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        EventId = eventId;
        OccurredAtUtc = occurredAtUtc;
        Source = source;
        Type = type;
        SyncStatus = syncStatus;
        Metadata = metadata ?? new Dictionary<string, string>();
    }

    /// <summary>شناسهٔ یکتای رویداد (کلید idempotency همگام‌سازی).</summary>
    public string EventId { get; }

    /// <summary>زمان وقوع به‌صورت UTC.</summary>
    public DateTimeOffset OccurredAtUtc { get; }

    /// <summary>منبع ثبت رویداد.</summary>
    public EventSource Source { get; }

    /// <summary>نوع رویداد.</summary>
    public EventType Type { get; }

    /// <summary>وضعیت همگام‌سازی.</summary>
    public SyncStatus SyncStatus { get; }

    /// <summary>ابردادهٔ محدود رویداد (مانند شدت، زمینه).</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
