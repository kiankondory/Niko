// ============================================================================
// Niko.Core — CompanionMessageType.cs
// ----------------------------------------------------------------------------
// مسئولیت: تعریف نوع پیام‌های ورودی/خروجی بین ابزارک/ساعت و هسته. این قرارداد
//           مستقل از پلتفرم و نسخه‌بندی‌شده است.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: مقادیر پایدارند؛ افزودن نوع جدید باید در DECISIONS.md ثبت شود.
// ============================================================================

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// نوع پیام قرارداد ابزارک/ساعت.
/// </summary>
public enum CompanionMessageType
{
    /// <summary>ثبت سریع (دود/مقاومت/هوس).</summary>
    QuickLog = 0,

    /// <summary>درخواست خلاصهٔ پیشرفت.</summary>
    ProgressSummaryRequest = 1,

    /// <summary>درخواست خلاصهٔ استریک/میل‌استون.</summary>
    StreakSummaryRequest = 2,

    /// <summary>درخواست وضعیت همگام‌سازی.</summary>
    SyncStatusRequest = 3,
}
