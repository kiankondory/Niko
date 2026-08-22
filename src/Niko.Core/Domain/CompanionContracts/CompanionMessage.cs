// ============================================================================
// Niko.Core — CompanionMessage.cs
// ----------------------------------------------------------------------------
// مسئولیت: قالب (envelope) نسخه‌بندی‌شدهٔ پیام ابزارک/ساعت. شامل نسخهٔ قرارداد،
//           شناسهٔ یکتا، منبع، نوع، محتوا (JSON) و زمان ارسال است.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: MessageId کلید idempotency است. قرارداد باید سازگار با
//           عقب‌ماندگی باشد و در DECISIONS.md ثبت شود.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// پیام قرارداد ابزارک/ساعت.
/// </summary>
public sealed record CompanionMessage
{
    /// <summary>نسخهٔ قرارداد (پیش‌فرض ۱).</summary>
    public int ContractVersion { get; init; } = 1;

    /// <summary>شناسهٔ یکتای پیام (برای جلوگیری از پردازش تکراری).</summary>
    public string MessageId { get; init; } = string.Empty;

    /// <summary>منبع پیام (ابزارک یا ساعت).</summary>
    public EventSource Source { get; init; }

    /// <summary>نوع پیام.</summary>
    public CompanionMessageType MessageType { get; init; }

    /// <summary>محتوای سریال‌شدهٔ پیام (JSON).</summary>
    public string Payload { get; init; } = string.Empty;

    /// <summary>زمان ارسال (UTC).</summary>
    public DateTimeOffset SentAtUtc { get; init; }
}
