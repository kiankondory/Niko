// ============================================================================
// Niko.Core — TriggerAnalysisUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: مورد کاربرد تحلیل محرک. اگر تحلیل غیرفعال باشد، هیچ داده‌ای خوانده یا
//           تحلیل نمی‌شود (حداقل دسترسی). اگر فعال باشد، رویدادهای محلی خوانده و
//           تحلیل خالص انجام می‌شود. فعال/غیرفعال شدن از طریق ذخیره‌ساز محلی ثبت می‌شود.
// وابستگی‌ها و لایه: UseCases/TriggerAnalysis → Abstractions (ITriggerAnalysisPreferenceStore,
//           ILocalStore) + Domain/TriggerAnalysis.
// نکات تغییر و قیود: کاملاً محلی و بدون ارسال بیرونی. حالت غیرفعال (پیش‌فرض امن)
//           داده‌ای را نمی‌خواند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.TriggerAnalysis;
using Niko.Core.Events;

namespace Niko.Core.UseCases.TriggerAnalysis;

/// <summary>
/// مورد کاربرد تحلیل محرک محلی.
/// </summary>
public sealed class TriggerAnalysisUseCase
{
    private const int BatchSize = 500;
    private readonly ITriggerAnalysisPreferenceStore _preferenceStore;
    private readonly ILocalStore _store;

    public TriggerAnalysisUseCase(
        ITriggerAnalysisPreferenceStore preferenceStore,
        ILocalStore store)
    {
        _preferenceStore = preferenceStore;
        _store = store;
    }

    /// <summary>بارگذاری ترجیح تحلیل؛ اگر ذخیره نشده باشد پیش‌فرض امن (غیرفعال).</summary>
    public async Task<TriggerAnalysisPreference> GetPreferenceAsync(CancellationToken ct = default)
    {
        return await _preferenceStore.GetAsync(ct).ConfigureAwait(false) ?? new TriggerAnalysisPreference();
    }

    /// <summary>فعال/غیرفعال کردن تحلیل و ذخیرهٔ ترجیح.</summary>
    public async Task SetEnabledAsync(bool enabled, CancellationToken ct = default)
    {
        await _preferenceStore.SaveAsync(new TriggerAnalysisPreference { Enabled = enabled }, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// اجرای تحلیل. اگر غیرفعال باشد، بدون خواندن رویدادها برمی‌گردد.
    /// </summary>
    public async Task<TriggerAnalysisResult> AnalyzeAsync(CancellationToken ct = default)
    {
        var preference = await GetPreferenceAsync(ct).ConfigureAwait(false);
        if (!preference.Enabled)
        {
            return new TriggerAnalysisResult { IsEnabled = false };
        }

        var events = await LoadAllEventsAsync(ct).ConfigureAwait(false);
        return TriggerAnalyzer.Analyze(events);
    }

    private async Task<IReadOnlyList<LogEvent>> LoadAllEventsAsync(CancellationToken ct)
    {
        var all = new List<LogEvent>();
        var offset = 0;

        while (true)
        {
            var batch = await _store.GetEventsAsync(offset, BatchSize, ct).ConfigureAwait(false);
            all.AddRange(batch);

            if (batch.Count < BatchSize)
            {
                break;
            }

            offset += batch.Count;
        }

        return all;
    }
}
