// ============================================================================
// Niko.Core — DashboardResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ مورد کاربرد داشبورد؛ شامل نمای پیشرفت و تعداد رویدادهای خوانده‌شده.
// وابستگی‌ها و لایه: بخش UseCases/Dashboard در Core.
// نکات تغییر و قیود: فقط دادهٔ نمایشی است؛ هیچ منطقی ندارد.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.CompanionContracts;

namespace Niko.Core.UseCases.Dashboard;

/// <summary>
/// نتیجهٔ خواندن داشبورد.
/// </summary>
public sealed record DashboardResult(
    ProgressSnapshot Snapshot,
    int EventCount,
    CompanionDailySummary DailySummary);
