// ============================================================================
// Niko.Core — DashboardUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: گردآوری رویدادها و پروفایل از ذخیره‌ساز محلی و تولید نمای پیشرفت
//           داشبورد با محاسبات دامنه (ProgressCalculator). کاملاً آفلاین است.
// وابستگی‌ها و لایه: UseCases/Dashboard → Abstractions (ILocalStore,
//           IUserSettingsStore) + Domain (ProgressCalculator).
// نکات تغییر و قیود: فقط خواندنی است و هیچ رویدادی ایجاد نمی‌کند. تاریخ «امروز»
//           از IClock می‌آید تا تست قطعی باشد. همهٔ محاسبات در Core انجام می‌شود.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain;
using Niko.Core.Events;

namespace Niko.Core.UseCases.Dashboard;

/// <summary>
/// مورد کاربرد خواندن داده‌های داشبورد از ذخیره‌ساز محلی.
/// </summary>
public sealed class DashboardUseCase
{
    private const int BatchSize = 500;
    private readonly ILocalStore _store;
    private readonly IUserSettingsStore _settingsStore;
    private readonly IClock _clock;

    public DashboardUseCase(
        ILocalStore store,
        IUserSettingsStore settingsStore,
        IClock clock)
    {
        _store = store;
        _settingsStore = settingsStore;
        _clock = clock;
    }

    public async Task<DashboardResult> ExecuteAsync(CancellationToken ct = default)
    {
        var events = await LoadAllEventsAsync(ct).ConfigureAwait(false);
        var profile = await _settingsStore.GetAsync(ct).ConfigureAwait(false);
        var today = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime);

        var snapshot = ProgressCalculator.Calculate(events, profile, today);

        return new DashboardResult(snapshot, events.Count);
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
