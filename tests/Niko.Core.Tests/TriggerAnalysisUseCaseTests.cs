// ============================================================================
// Niko.Core.Tests — TriggerAnalysisUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون رضایت کاربر، حداقل دسترسی و رفتار امن مورد کاربرد تحلیل محرک.
// وابستگی‌ها و لایه: تست Core؛ UseCase را با ذخیره‌سازهای درون‌حافظه‌ای آزمایش می‌کند.
// نکات تغییر و قیود: تحلیل فقط محلی است؛ در حالت غیرفعال نباید رویدادها خوانده شوند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.TriggerAnalysis;
using Niko.Core.Events;
using Niko.Core.UseCases.TriggerAnalysis;

namespace Niko.Core.Tests;

public sealed class TriggerAnalysisUseCaseTests
{
    [Fact]
    public async Task AnalyzeAsync_WhenDisabled_DoesNotReadEvents()
    {
        var preferences = new InMemoryTriggerAnalysisPreferenceStore();
        var store = new CountingLocalStore();
        var useCase = new TriggerAnalysisUseCase(preferences, store);

        var result = await useCase.AnalyzeAsync();

        Assert.False(result.IsEnabled);
        Assert.False(result.HasSufficientData);
        Assert.Equal(0, store.ReadCount);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenEnabled_ReadsEvents()
    {
        var preferences = new InMemoryTriggerAnalysisPreferenceStore();
        var store = new CountingLocalStore(CreateEvents(5));
        var useCase = new TriggerAnalysisUseCase(preferences, store);
        await useCase.SetEnabledAsync(true);

        var result = await useCase.AnalyzeAsync();

        Assert.True(result.IsEnabled);
        Assert.Equal(1, store.ReadCount);
        Assert.Equal(5, result.TotalEventsAnalyzed);
    }

    [Fact]
    public async Task SetEnabledAsync_PersistsPreference()
    {
        var preferences = new InMemoryTriggerAnalysisPreferenceStore();
        var useCase = new TriggerAnalysisUseCase(preferences, new CountingLocalStore());

        await useCase.SetEnabledAsync(true);
        var enabled = await useCase.GetPreferenceAsync();
        await useCase.SetEnabledAsync(false);
        var disabled = await useCase.GetPreferenceAsync();

        Assert.True(enabled.Enabled);
        Assert.False(disabled.Enabled);
        Assert.Equal(2, preferences.SaveCount);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenEnabledWithInsufficientData_ReturnsMinimumDataResult()
    {
        var preferences = new InMemoryTriggerAnalysisPreferenceStore();
        var store = new CountingLocalStore(CreateEvents(TriggerAnalysisResult.MinimumDataThreshold - 1));
        var useCase = new TriggerAnalysisUseCase(preferences, store);
        await useCase.SetEnabledAsync(true);

        var result = await useCase.AnalyzeAsync();

        Assert.True(result.IsEnabled);
        Assert.False(result.HasSufficientData);
        Assert.Equal(TriggerAnalysisResult.MinimumDataThreshold - 1, result.TotalEventsAnalyzed);
        Assert.Empty(result.Insights);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenEnabledWithNoEvents_ReturnsSafeEmptyResult()
    {
        var preferences = new InMemoryTriggerAnalysisPreferenceStore();
        var useCase = new TriggerAnalysisUseCase(preferences, new CountingLocalStore());
        await useCase.SetEnabledAsync(true);

        var result = await useCase.AnalyzeAsync();

        Assert.True(result.IsEnabled);
        Assert.False(result.HasSufficientData);
        Assert.Equal(0, result.TotalEventsAnalyzed);
        Assert.Empty(result.Insights);
    }

    private static IReadOnlyList<LogEvent> CreateEvents(int count)
        => Enumerable.Range(0, count)
            .Select(index => new LogEvent(
                $"use-case-event-{index}",
                new DateTimeOffset(2024, 1, 8, 10, index, 0, TimeSpan.Zero),
                EventSource.Mobile,
                EventType.Craving,
                SyncStatus.Pending))
            .ToList();

    private sealed class CountingLocalStore : ILocalStore
    {
        private readonly IReadOnlyList<LogEvent> _events;

        public CountingLocalStore(IReadOnlyList<LogEvent>? events = null)
        {
            _events = events ?? Array.Empty<LogEvent>();
        }

        public int ReadCount { get; private set; }

        public Task SaveEventAsync(LogEvent logEvent, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task<IReadOnlyList<LogEvent>> GetEventsAsync(
            int offset = 0,
            int limit = 100,
            CancellationToken ct = default)
        {
            ReadCount++;
            return Task.FromResult<IReadOnlyList<LogEvent>>(_events.Skip(offset).Take(limit).ToList());
        }

        public Task<IReadOnlyList<LogEvent>> GetPendingEventsAsync(
            int limit = 100,
            CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<LogEvent>>(Array.Empty<LogEvent>());

        public Task UpdateSyncStatusAsync(
            string eventId,
            SyncStatus status,
            CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
