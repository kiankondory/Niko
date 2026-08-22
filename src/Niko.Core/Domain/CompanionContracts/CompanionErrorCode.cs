// ============================================================================
// Niko.Core — CompanionErrorCode.cs
// ----------------------------------------------------------------------------
// مسئولیت: کدهای خطای امن برای پاسخ به پیام‌های ابزارک/ساعت. خطاها به‌صورت ساختاری
//           بازمی‌گردند تا ابزارک بتواند وضعیت ناموفق را بدون منطق دامنه نمایش دهد.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core.
// نکات تغییر و قیود: مقادیر پایدارند؛ پیام‌های خطا در منابع locale هستند.
// ============================================================================

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// کد خطای نتیجهٔ پردازش پیام.
/// </summary>
public enum CompanionErrorCode
{
    /// <summary>بدون خطا.</summary>
    None = 0,

    /// <summary>نسخهٔ قرارداد پشتیبانی نمی‌شود.</summary>
    UnsupportedVersion = 1,

    /// <summary>محتوای پیام ناقص/نامعتبر است.</summary>
    MalformedPayload = 2,

    /// <summary>منبع پیام نامعتبر است.</summary>
    InvalidSource = 3,

    /// <summary>پیام تکراری است (MessageId قبلاً پردازش شده).</summary>
    DuplicateEvent = 4,

    /// <summary>نوع پیام ناشناخته است.</summary>
    UnknownMessage = 5,

    /// <summary>اعتبارسنجی ورودی شکست خورد.</summary>
    ValidationFailed = 6,
}
