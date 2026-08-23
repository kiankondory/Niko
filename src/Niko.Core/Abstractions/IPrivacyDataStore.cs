// ============================================================================
// Niko.Core — IPrivacyDataStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد export و پاک‌سازی کامل داده‌های محلی با رضایت صریح کاربر.
// وابستگی‌ها و لایه: Core abstraction؛ پیاده‌سازی SQLite در Infrastructure است.
// نکات تغییر و قیود: هیچ ارسال شبکه‌ای ندارد؛ erase باید اتمیک باشد.
// ============================================================================

namespace Niko.Core.Abstractions;

public interface IPrivacyDataStore
{
    Task<string> ExportJsonAsync(CancellationToken ct = default);
    Task EraseAllAsync(CancellationToken ct = default);
}
