// ============================================================================
// Niko.Core.Tests — QuickLogUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های مورد کاربرد ثبت سریع: پذیرش انواع مجاز، ذخیرهٔ محلی با وضعیت
//           Pending، ثبت زمان UTC، و رد کردن انواع نامجاز.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی و مستقل از شبکه/زمان واقعی هستند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Events;
using Niko.Core.UseCases.QuickLog;

namespace Niko.Core.Tests;

public class QuickLogUseCaseTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryStore _store;
    private readonly QuickLogUseCase _useCase;

    public QuickLogUseCaseTests()
    {
        _clock = new FakeClock();
        _store = new InMemoryStore();
        _useCase = new QuickLogUseCase(_store, _clock);
    }

    [Theory]
    [InlineData(EventType.Smoked)]
    [InlineData(EventType.Resisted)]
    [InlineData(EventType.Craving)]
    public async Task Execute_WithAllowedType_SavesPendingEvent(EventType type)
    {
        var result = await _useCase.ExecuteAsync(new QuickLogRequest(type));

        Assert.Equal(type, result.Type);
        Assert.Equal(SyncStatus.Pending, result.SyncStatus);
        Assert.Single(_store.Events);
        Assert.Equal(SyncStatus.Pending, _store.Events[0].SyncStatus);
    }

    [Fact]
    public async Task Execute_WithOccurredAt_UsesProvidedUtcTime()
    {
        var utc = new DateTimeOffset(2024, 3, 5, 12, 30, 0, TimeSpan.Zero);

        await _useCase.ExecuteAsync(new QuickLogRequest(EventType.Smoked, OccurredAtUtc: utc));

        Assert.Equal(utc, _store.Events[0].OccurredAtUtc);
        Assert.Equal(utc.Offset, _store.Events[0].OccurredAtUtc.Offset);
    }

    [Fact]
    public async Task Execute_WithoutOccurredAt_UsesClockUtc()
    {
        await _useCase.ExecuteAsync(new QuickLogRequest(EventType.Resisted));

        Assert.Equal(_clock.UtcNow, _store.Events[0].OccurredAtUtc);
    }

    [Fact]
    public async Task Execute_WithIntensityAndContext_StoresMetadata()
    {
        await _useCase.ExecuteAsync(new QuickLogRequest(
            EventType.Craving,
            Intensity: 7,
            Context: "stress"));

        var metadata = _store.Events[0].Metadata;
        Assert.Equal("7", metadata["intensity"]);
        Assert.Equal("stress", metadata["context"]);
    }

    [Fact]
    public async Task Execute_WithExplicitSameEventId_IsIdempotent()
    {
        var request = new QuickLogRequest(EventType.Smoked, EventId: "event-1");
        var first = await _useCase.ExecuteAsync(request);
        var second = await _useCase.ExecuteAsync(request);

        Assert.Equal("event-1", first.EventId);
        Assert.Equal(first.EventId, second.EventId);
        Assert.Single(_store.Events);
    }

    [Fact]
    public async Task Execute_WithDisallowedType_Throws()
    {
        var request = new QuickLogRequest(EventType.ProfileCreated);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecuteAsync(request));
    }

    [Fact]
    public async Task Execute_DefaultSource_IsMobile()
    {
        await _useCase.ExecuteAsync(new QuickLogRequest(EventType.Smoked));

        Assert.Equal(EventSource.Mobile, _store.Events[0].Source);
    }
}
