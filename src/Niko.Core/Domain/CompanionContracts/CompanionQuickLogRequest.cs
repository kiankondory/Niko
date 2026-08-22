// ============================================================================
// Niko.Core — CompanionQuickLogRequest.cs
// ----------------------------------------------------------------------------
// مسئولیت: محتوای پیام ثبت سریع از ابزارک/ساعت. شامل نوع رویداد، زمان، شدت/زمینه
//           و شناسهٔ رویداد اختیاری است.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core.
// نکات تغییر و قیود: فقط سه نوع اصلی (دود/مقاومت/هوس) پذیرفته می‌شود؛ بقیه در
//           هسته رد می‌شوند. قرارداد نسخه‌بندی‌شده است.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// درخواست ثبت سریع از ابزارک/ساعت.
/// </summary>
public sealed record CompanionQuickLogRequest
{
    /// <summary>نوع رویداد.</summary>
    public EventType EventType { get; init; }

    /// <summary>زمان وقوع (اختیاری؛ در صورت نبود، از ساعت هسته استفاده می‌شود).</summary>
    public DateTimeOffset? OccurredAtUtc { get; init; }

    /// <summary>شدت (اختیاری).</summary>
    public int? Intensity { get; init; }

    /// <summary>زمینه (اختیاری).</summary>
    public string? Context { get; init; }

    /// <summary>شناسهٔ رویداد (اختیاری؛ در صورت نبود، هسته تولید می‌کند).</summary>
    public string? EventId { get; init; }
}
