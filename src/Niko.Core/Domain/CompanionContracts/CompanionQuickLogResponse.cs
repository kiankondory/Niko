// ============================================================================
// Niko.Core — CompanionQuickLogResponse.cs
// ----------------------------------------------------------------------------
// مسئولیت: پاسخ ثبت سریع به ابزارک/ساعت؛ شامل وضعیت موفقیت، شناسهٔ رویداد و
//           وضعیت همگام‌سازی. فقط دادهٔ نمایشی است.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core.
// نکات تغییر و قیود: SyncStatus به‌صورت پایدار نگاشت می‌شود.
// ============================================================================

using Niko.Core.Events;

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// پاسخ ثبت سریع.
/// </summary>
public sealed record CompanionQuickLogResponse(
    bool Success,
    string EventId,
    SyncStatus SyncStatus);
