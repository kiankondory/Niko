// ============================================================================
// Niko.Core.Tests — CoachUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون رفتار امن، ترجیحات و fallback قطعی مربی محلی.
// وابستگی‌ها و لایه: تست Core؛ از store درون‌حافظه‌ای استفاده می‌کند و شبکه ندارد.
// نکات تغییر و قیود: خروجی فقط کلید محلی‌سازی است و هیچ متن یا رویداد خصوصی آزمون
//           داده نمی‌شود؛ پیش‌فرض مربی خاموش باقی می‌ماند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;
using Niko.Core.UseCases.Coach;

namespace Niko.Core.Tests;

public sealed class CoachUseCaseTests
{
    [Fact]
    public async Task GenerateAsync_DefaultPreferences_IsDisabled()
    {
        var useCase = new CoachUseCase(new InMemoryStore());

        var response = await useCase.GenerateAsync(CoachRequest.Local(new CoachContext(7, null, null, null, Array.Empty<string>())));

        Assert.False(response.IsEnabled);
        Assert.Equal(CoachSafetyStatus.Disabled, response.SafetyStatus);
        Assert.Empty(response.Suggestions);
    }

    [Fact]
    public async Task GenerateAsync_EnabledWithEmptyContext_ReturnsSafeEmptyResult()
    {
        var store = new InMemoryStore(new CoachPreferences { Enabled = true });
        var response = await new CoachUseCase(store).GenerateAsync(CoachRequest.Local(CoachContext.Empty));

        Assert.Equal(CoachSafetyStatus.EmptyContext, response.SafetyStatus);
        Assert.Equal(CoachProviderError.EmptyContext, response.ProviderResult.Error);
        Assert.Empty(response.Suggestions);
    }

    [Fact]
    public async Task GenerateAsync_AllowedCravingContext_ReturnsCravingSuggestion()
    {
        var store = new InMemoryStore(new CoachPreferences { Enabled = true, AllowCravingContext = true });
        var response = await new CoachUseCase(store).GenerateAsync(
            CoachRequest.Local(new CoachContext(7, null, "breathing", null, Array.Empty<string>())));

        Assert.Equal(CoachSafetyStatus.Safe, response.SafetyStatus);
        var suggestion = Assert.Single(response.Suggestions);
        Assert.Equal("Coach.Suggestion.CravingSupport", suggestion.TextKey);
    }

    [Fact]
    public async Task GenerateAsync_DisallowedCravingContext_DoesNotExposeIt()
    {
        var store = new InMemoryStore(new CoachPreferences { Enabled = true });
        var response = await new CoachUseCase(store).GenerateAsync(
            CoachRequest.Local(new CoachContext(7, null, null, null, Array.Empty<string>())));

        Assert.Equal(CoachSafetyStatus.EmptyContext, response.SafetyStatus);
        Assert.Empty(response.Suggestions);
    }

    [Fact]
    public async Task GenerateAsync_SameAggregateContext_IsDeterministic()
    {
        var store = new InMemoryStore(new CoachPreferences
        {
            Enabled = true,
            AllowCravingContext = true,
            AllowAggregatedProgressContext = true,
        });
        var useCase = new CoachUseCase(store);
        var request = CoachRequest.Local(new CoachContext(4, 35, null, "week-one", Array.Empty<string>()));

        var first = await useCase.GenerateAsync(request);
        var second = await useCase.GenerateAsync(request);

        Assert.Equal(first.IsEnabled, second.IsEnabled);
        Assert.Equal(first.SafetyStatus, second.SafetyStatus);
        Assert.Equal(
            first.Suggestions.Select(s => (s.Id, s.TextKey, s.Kind)),
            second.Suggestions.Select(s => (s.Id, s.TextKey, s.Kind)));
    }

    [Fact]
    public void Policy_RejectsUnsafeContextAndOutput()
    {
        Assert.False(CoachPolicy.IsContextAllowed(new CoachContext(11, null, null, null, Array.Empty<string>())));
        Assert.False(CoachPolicy.IsContextAllowed(new CoachContext(null, null, null, null, Enumerable.Repeat("x", 9).ToArray())));
        Assert.False(CoachPolicy.IsProviderTextAllowed("This is a diagnosis and a guaranteed outcome."));
        Assert.True(CoachPolicy.IsProviderTextAllowed("A short supportive suggestion."));
    }

    private sealed class InMemoryStore(CoachPreferences? initial = null) : ICoachPreferencesStore
    {
        private CoachPreferences? _preferences = initial;

        public Task<CoachPreferences?> GetAsync(CancellationToken ct = default)
            => Task.FromResult(_preferences);

        public Task SaveAsync(CoachPreferences preferences, CancellationToken ct = default)
        {
            _preferences = preferences;
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken ct = default)
        {
            _preferences = null;
            return Task.CompletedTask;
        }
    }
}
