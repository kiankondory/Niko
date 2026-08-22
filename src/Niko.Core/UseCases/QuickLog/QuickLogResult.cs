// ============================================================================
// Niko.Core — QuickLogResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ ثبت سریع؛ شامل شناسهٔ رویداد و وضعیت نهایی.
// وابستگی‌ها و لایه: بخش UseCases/QuickLog در Core.
// نکات تغییر و قیود: وضعیت نشان می‌دهد رویداد محلی ذخیره و در صف همگام‌سازی است.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.UseCases.QuickLog;

/// <summary>
/// نتیجهٔ ثبت سریع رویداد.
/// </summary>
public sealed record QuickLogResult(
    string EventId,
    EventType Type,
    SyncStatus SyncStatus);
