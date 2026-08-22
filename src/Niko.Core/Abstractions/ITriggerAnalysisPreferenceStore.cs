// ============================================================================
// Niko.Core — ITriggerAnalysisPreferenceStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد ذخیره/بازیابی ترجیح تحلیل محرک. پیاده‌سازی (SQLite) در
//           Infrastructure است؛ هسته فقط قرارداد را می‌شناسد.
// وابستگی‌ها و لایه: بخش Abstractions در Core.
// نکات تغییر و قیود: ذخیره محلی و آفلاین است. اگر هیچ ترجیحی ذخیره نشده باشد،
//           پیش‌فرض امن (غیرفعال) برمی‌گردد.
// ============================================================================

using Niko.Core.Domain.TriggerAnalysis;

namespace Niko.Core.Abstractions;

/// <summary>
/// قرارداد ذخیره‌سازی ترجیح تحلیل محرک.
/// </summary>
public interface ITriggerAnalysisPreferenceStore
{
    /// <summary>بازیابی ترجیح؛ اگر ذخیره نشده باشد null برمی‌گرداند.</summary>
    Task<TriggerAnalysisPreference?> GetAsync(CancellationToken ct = default);

    /// <summary>ذخیره یا به‌روزرسانی ترجیح.</summary>
    Task SaveAsync(TriggerAnalysisPreference preference, CancellationToken ct = default);
}
