// ============================================================================
// Niko.Core.Tests — InMemoryTriggerAnalysisPreferenceStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: بدل درون‌حافظه‌ای قرارداد ترجیح تحلیل محرک برای تست مورد کاربرد.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده‌سازی می‌کند و به تولید وصل نیست.
// نکات تغییر و قیود: فقط وضعیت محلی و تعداد ذخیره‌سازی را نگه می‌دارد؛ بدون شبکه و دادهٔ خام.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.TriggerAnalysis;

namespace Niko.Core.Tests;

public sealed class InMemoryTriggerAnalysisPreferenceStore : ITriggerAnalysisPreferenceStore
{
    public TriggerAnalysisPreference? Preference { get; private set; }

    public int SaveCount { get; private set; }

    public Task<TriggerAnalysisPreference?> GetAsync(CancellationToken ct = default)
        => Task.FromResult(Preference);

    public Task SaveAsync(TriggerAnalysisPreference preference, CancellationToken ct = default)
    {
        Preference = preference;
        SaveCount++;
        return Task.CompletedTask;
    }
}
