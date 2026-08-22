// ============================================================================
// Niko.Core — NotificationContent.cs
// ----------------------------------------------------------------------------
// مسئولیت: محتوای یک اعلان به‌صورت کلیدهای محلی‌سازی + پارامترهای ساخت‌یافته.
//           هسته فقط کلید و پارامتر می‌شناسد؛ متن واقعی در منابع locale است.
// وابستگی‌ها و لایه: بخش Domain/Notifications در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: محتوا کوتاه، حمایتی و بدون دادهٔ حساس در پیش‌نمایش است.
// ============================================================================

namespace Niko.Core.Domain.Notifications;

/// <summary>
/// محتوای اعلان با کلیدهای محلی‌سازی و پارامترهای اختیاری.
/// </summary>
public sealed record NotificationContent(
    string TitleKey,
    string BodyKey,
    IReadOnlyDictionary<string, string>? TitleArgs = null,
    IReadOnlyDictionary<string, string>? BodyArgs = null);
