// ============================================================================
// Niko.Core.Tests — ExternalCoachPrivacyGatewayTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون رضایت، redaction، خطاهای provider و fallback امن gateway خارجی.
// وابستگی‌ها و لایه: تست Core با provider جعلی درون‌حافظه‌ای؛ بدون شبکه و secret.
// نکات تغییر و قیود: provider فقط ApprovedCoachContext می‌بیند و همهٔ خروجی‌های
//           unsafe یا خطادار به پیشنهاد محلی محدود برمی‌گردند.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;
using Niko.Core.UseCases.Coach;

namespace Niko.Core.Tests;

public sealed class ExternalCoachPrivacyGatewayTests
{
    [Fact]
    public async Task GenerateAsync_UnknownBillingState_FailsClosed()
    {
        var provider = new FakeProvider();
        var gateway = new ExternalCoachPrivacyGateway(
            new InMemoryStore(EnabledWithCravingConsent()),
            provider,
            new ExternalCoachProviderConfiguration(Enabled: true));

        var result = await gateway.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.Unavailable, result.Error);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_ProviderUnavailable_FailsClosed()
    {
        var provider = new FakeProvider
        {
            Availability = ExternalCoachAvailability.FailClosed(ExternalCoachAvailabilityState.FreeQuotaUnavailable),
        };
        var gateway = CreateGateway(provider, EnabledWithCravingConsent());

        var result = await gateway.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.Unavailable, result.Error);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_Disabled_DoesNotCallProvider()
    {
        var provider = new FakeProvider();
        var gateway = CreateGateway(provider, new CoachPreferences());

        var result = await gateway.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.Disabled, result.Error);
        Assert.Equal(0, provider.CallCount);
        Assert.NotNull(result.LocalFallback);
    }

    [Fact]
    public async Task GenerateAsync_WithoutExternalConsent_DoesNotCallProvider()
    {
        var provider = new FakeProvider();
        var gateway = CreateGateway(provider, new CoachPreferences { Enabled = true });

        var result = await gateway.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.ConsentRequired, result.Error);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task GenerateAsync_ForwardsOnlyApprovedAggregateContext()
    {
        var provider = new FakeProvider
        {
            Result = SuccessResult(),
        };
        var gateway = CreateGateway(provider, new CoachPreferences
        {
            Enabled = true,
            AllowExternalProvider = true,
            AllowAggregatedProgressContext = true,
            AllowCravingContext = false,
        });

        var result = await gateway.GenerateAsync(new CoachRequest(
            new CoachContext(8, 42, "breathing", "milestone-one", new[] { "short preference" }),
            true,
            TimeSpan.FromSeconds(1)));

        Assert.True(result.Succeeded);
        Assert.NotNull(provider.LastRequest);
        Assert.Null(provider.LastRequest!.Context.CravingIntensity);
        Assert.Equal(42, provider.LastRequest.Context.ProgressPercent);
        Assert.Equal("breathing", provider.LastRequest.Context.SelectedIntervention);
        Assert.Equal("milestone-one", provider.LastRequest.Context.MilestoneStatus);
        Assert.Equal(new[] { "short preference" }, provider.LastRequest.Context.UserPreferences);
    }

    [Fact]
    public async Task GenerateAsync_UnsafeProviderOutput_ReturnsLocalFallback()
    {
        var provider = new FakeProvider
        {
            Result = new ExternalCoachResult(
                true,
                ExternalCoachError.None,
                new ExternalCoachResponse("This is a diagnosis.", ExternalCoachSafetyResult.Allowed),
                CoachProviderResult.Failure(CoachProviderError.None)),
        };
        var gateway = CreateGateway(provider, EnabledWithCravingConsent());

        var result = await gateway.GenerateAsync(Request());

        Assert.False(result.Succeeded);
        Assert.Equal(ExternalCoachError.UnsafeOutput, result.Error);
        Assert.Null(result.Response);
        Assert.Contains(result.LocalFallback.Suggestions, suggestion =>
            suggestion.TextKey == "Coach.Suggestion.CravingSupport");
    }

    [Fact]
    public async Task GenerateAsync_Timeout_ReturnsLocalFallback()
    {
        var provider = new FakeProvider { WaitForCancellation = true };
        var gateway = CreateGateway(EnabledWithCravingConsent(), provider, TimeSpan.FromMilliseconds(20));

        var result = await gateway.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.Timeout, result.Error);
        Assert.NotEmpty(result.LocalFallback.Suggestions);
    }

    [Fact]
    public async Task GenerateAsync_RateLimit_ReturnsLocalFallback()
    {
        var provider = new FakeProvider
        {
            Result = ExternalCoachResult.Failure(
                ExternalCoachError.RateLimited,
                CoachProviderResult.Failure(CoachProviderError.Unavailable)),
        };
        var gateway = CreateGateway(provider, EnabledWithCravingConsent());

        var result = await gateway.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.RateLimited, result.Error);
        Assert.NotEmpty(result.LocalFallback.Suggestions);
    }

    [Fact]
    public async Task GenerateAsync_Cancellation_ReturnsSafeCancellationResult()
    {
        var provider = new FakeProvider { WaitForCancellation = true };
        var gateway = CreateGateway(provider, EnabledWithCravingConsent());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await gateway.GenerateAsync(Request(), cts.Token);

        Assert.Equal(ExternalCoachError.Cancelled, result.Error);
        Assert.NotEmpty(result.LocalFallback.Suggestions);
    }

    [Fact]
    public async Task GenerateAsync_InvalidContext_IsRejectedBeforeProvider()
    {
        var provider = new FakeProvider();
        var gateway = CreateGateway(provider, EnabledWithCravingConsent());

        var result = await gateway.GenerateAsync(new CoachRequest(
            new CoachContext(7, null, new string('x', 41), null, Array.Empty<string>()),
            true,
            TimeSpan.FromSeconds(1)));

        Assert.Equal(ExternalCoachError.PayloadTooLarge, result.Error);
        Assert.Equal(0, provider.CallCount);
    }

    private static ExternalCoachPrivacyGateway CreateGateway(
        FakeProvider provider,
        CoachPreferences preferences)
        => CreateGateway(provider, preferences, TimeSpan.FromSeconds(1));

    private static ExternalCoachPrivacyGateway CreateGateway(
        FakeProvider provider,
        CoachPreferences preferences,
        TimeSpan timeout)
        => new(new InMemoryStore(preferences), provider,
            new ExternalCoachProviderConfiguration(
                Enabled: true,
                Timeout: timeout,
                MaxResponseCharacters: 500,
                BillingExplicitlyDisabled: true,
                PaidFallbackConfigured: false));

    private static ExternalCoachPrivacyGateway CreateGateway(
        CoachPreferences preferences,
        FakeProvider provider,
        TimeSpan timeout)
        => CreateGateway(provider, preferences, timeout);

    private static CoachRequest Request()
        => new(new CoachContext(7, null, "breathing", null, Array.Empty<string>()), true, TimeSpan.FromSeconds(1));

    private static CoachPreferences EnabledWithCravingConsent()
        => new() { Enabled = true, AllowExternalProvider = true, AllowCravingContext = true };

    private static ExternalCoachResult SuccessResult()
        => new(true, ExternalCoachError.None,
            new ExternalCoachResponse("A short supportive suggestion.", ExternalCoachSafetyResult.Allowed),
            CoachProviderResult.Failure(CoachProviderError.None));

    private sealed class FakeProvider : IExternalCoachProvider
    {
        public ExternalCoachResult Result { get; init; } = SuccessResult();
        public bool WaitForCancellation { get; init; }
        public int CallCount { get; private set; }
        public ExternalCoachRequest? LastRequest { get; private set; }
        public ExternalCoachAvailability Availability { get; init; } = new(
            ExternalCoachAvailabilityState.AvailableFree,
            IsFree: true,
            BillingDisabled: true,
            HasPaidFallback: false);

        public Task<ExternalCoachAvailability> GetAvailabilityAsync(CancellationToken ct = default)
            => Task.FromResult(Availability);

        public async Task<ExternalCoachResult> GenerateAsync(
            ExternalCoachRequest request,
            CancellationToken ct = default)
        {
            CallCount++;
            LastRequest = request;
            if (WaitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            }

            return Result;
        }
    }

    private sealed class InMemoryStore(CoachPreferences preferences) : ICoachPreferencesStore
    {
        public Task<CoachPreferences?> GetAsync(CancellationToken ct = default)
            => Task.FromResult<CoachPreferences?>(preferences);

        public Task SaveAsync(CoachPreferences preferences, CancellationToken ct = default)
            => Task.CompletedTask;

        public Task ClearAsync(CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
