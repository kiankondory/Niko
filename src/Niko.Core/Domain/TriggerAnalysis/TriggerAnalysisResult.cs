// ============================================================================
// Niko.Core — TriggerAnalysisResult.cs
// ----------------------------------------------------------------------------
// مسئولیت: نتیجهٔ تحلیل محرک. شامل وضعیت فعال بودن، کفایت داده، تعداد رویدادهای
//           تحلیل‌شده و فهرست بینش‌های تلفیقی است.
// وابستگی‌ها و لایه: بخش Domain/TriggerAnalysis در Core.
// نکات تغییر و قیود: اگر تحلیل غیرفعال یا داده ناکافی باشد، بینشی برنمی‌گردد.
// ============================================================================

namespace Niko.Core.Domain.TriggerAnalysis;

/// <summary>
/// نتیجهٔ تحلیل محرک.
/// </summary>
public sealed record TriggerAnalysisResult
{
    /// <summary>آیا تحلیل توسط کاربر فعال شده است؟</summary>
    public bool IsEnabled { get; init; }

    /// <summary>آیا دادهٔ کافی برای نتیجهٔ معنی‌دار وجود دارد؟</summary>
    public bool HasSufficientData { get; init; }

    /// <summary>تعداد رویدادهای تحلیل‌شده (فقط تجمیع؛ نه جزئیات).</summary>
    public int TotalEventsAnalyzed { get; init; }

    /// <summary>فهرست بینش‌های تلفیقی.</summary>
    public IReadOnlyList<TriggerInsight> Insights { get; init; } = Array.Empty<TriggerInsight>();

    /// <summary>حداقل تعداد رویداد برای نتیجهٔ معنی‌دار.</summary>
    public const int MinimumDataThreshold = 5;
}
