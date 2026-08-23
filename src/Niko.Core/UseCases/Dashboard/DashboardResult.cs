// ============================================================================
// Niko.Core — DashboardResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ مورد کاربرد داشبورد؛ شامل نمای پیشرفت و تعداد رویدادهای خوانده‌شده.
// وابستگی‌ها و لایه: بخش UseCases/Dashboard در Core.
// نکات تغییر و قیود: فقط دادهٔ نمایشی است؛ هیچ منطقی ندارد.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.CompanionContracts;
using Niko.Core.Domain.Island;

namespace Niko.Core.UseCases.Dashboard;

/// <summary>
/// نتیجهٔ خواندن داشبورد.
/// </summary>
public sealed record DashboardResult(
    ProgressSnapshot Snapshot,
    int EventCount,
    CompanionDailySummary DailySummary)
{
    /// <summary>
    /// صرفه‌جویی تقریبی امروز از مقاومت‌های معتبر همان روز و قیمت هر نخ.
    /// نبودن قیمت معتبر با null نمایش داده می‌شود.
    /// </summary>
    public decimal? DailySavedAmount { get; init; }

    /// <summary>کد ارز مبلغ روزانه، فقط در صورت وجود مبلغ.</summary>
    public string? DailySavingsCurrencyCode { get; init; }

    /// <summary>
    /// ارزش تقریبی هر نخِ مقاومت‌شده بر پایهٔ قیمت مؤثر پروفایل. این مقدار
    /// اختیاری است تا presentation بتواند بدون رویداد خام، ارزش هر اقدام را نشان دهد.
    /// </summary>
    public decimal? AmountPerResistedCigarette { get; init; }

    /// <summary>گزارش روزانهٔ تجمیعی جزیره از تاریخ ترک تا امروز.</summary>
    public IReadOnlyList<IslandDailyReport> IslandDailyReports { get; init; } =
        Array.Empty<IslandDailyReport>();

    /// <summary>مجموع پس‌انداز مقاومت‌ها از تاریخ ترک تا امروز.</summary>
    public decimal? IslandCumulativeSavings { get; init; }
}
