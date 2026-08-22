// ============================================================================
// Niko.Core.Tests — CravingBattleUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های مورد کاربرد نبرد با هوس: پایداری رویدادها در ذخیره‌ساز محلی
//           (آفلاین) و رفتار نهایی مقاومت/مصرف برای شمارش در داشبورد.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و از FakeClock و InMemoryStore استفاده می‌کنند.
// ============================================================================

using Niko.Core.Domain.Craving;
using Niko.Core.Events;
using Niko.Core.UseCases.CravingBattle;

namespace Niko.Core.Tests;

public class CravingBattleUseCaseTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryStore _store;
    private readonly CravingBattleUseCase _useCase;

    public CravingBattleUseCaseTests()
    {
        _clock = new FakeClock();
        _store = new InMemoryStore();
        _useCase = new CravingBattleUseCase(_store, _clock);
    }

    [Fact]
    public async Task Start_PersistsStartedAndCravingEvents()
    {
        var result = await _useCase.StartAsync(CravingIntensity.Mild);

        Assert.Equal(CravingBattleState.Started, result.State);

        var events = _store.Events;
        // ۱ رویداد نبرد (started) + ۱ رویداد هوس
        Assert.Equal(2, events.Count);
        Assert.Contains(events, e => e.Type == EventType.CravingAction);
        Assert.Contains(events, e => e.Type == EventType.Craving);
        Assert.All(events, e => Assert.Equal(SyncStatus.Pending, e.SyncStatus));
    }

    [Fact]
    public async Task FullFlow_PersistsStageEvents_AndEndsCompleted()
    {
        await _useCase.StartAsync(CravingIntensity.Moderate);
        await _useCase.SelectActionAsync(Intervention.DeepBreathing);
        var result = await _useCase.CompleteAsync();

        Assert.Equal(CravingBattleState.Completed, result.State);

        var cravingActionEvents = _store.Events.Where(e => e.Type == EventType.CravingAction).ToList();
        Assert.Equal(3, cravingActionEvents.Count); // started, action_selected, completed
        Assert.Equal(new[] { "started", "action_selected", "completed" },
            cravingActionEvents.Select(e => e.Metadata["stage"]));
    }

    [Fact]
    public async Task Resist_PersistsResistedOutcomeAndEvent()
    {
        await _useCase.StartAsync(CravingIntensity.Intense);
        var result = await _useCase.ResistAsync();

        Assert.Equal(CravingBattleState.Resisted, result.State);
        Assert.Contains(_store.Events, e => e.Type == EventType.Resisted);
    }

    [Fact]
    public async Task ExitSmoked_PersistsSmokedOutcomeAndEvent()
    {
        await _useCase.StartAsync(CravingIntensity.Mild);
        var result = await _useCase.ExitSmokedAsync();

        Assert.Equal(CravingBattleState.ExitedSmoked, result.State);
        Assert.Contains(_store.Events, e => e.Type == EventType.Smoked);
    }

    [Fact]
    public async Task SelectAction_BeforeStart_Throws()
    {
        // بدون Start، هیچ نبرد فعالی نیست.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.SelectActionAsync(Intervention.Delay));
    }
}
