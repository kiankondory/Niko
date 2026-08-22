// ============================================================================
// نام فایل: BackendCoachProxyProviderTests.cs
// مسئولیت: آزمون deterministic adapter موبایل به proxy و مسیرهای failure/fallback.
// وابستگی‌ها و لایه: Infrastructure.Tests → Core contracts؛ بدون شبکهٔ واقعی یا secret.
// نکات تغییر و قیود: endpoint جعلی فقط درخواست redacted و خطای rate-limit را شبیه‌سازی می‌کند.
// ============================================================================

using System.Net;
using System.Net.Http.Json;
using Niko.Core.Domain.Coach;
using Niko.Infrastructure.Coach;

namespace Niko.Infrastructure.Tests;

public sealed class BackendCoachProxyProviderTests
{
    [Fact]
    public async Task MissingRuntimeConfigurationReturnsUnavailable()
    {
        using var client = new HttpClient(new StubHandler());
        var provider = new BackendCoachProxyProvider(client, null, null, null);

        var result = await provider.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.Unavailable, result.Error);
    }

    [Fact]
    public async Task RateLimitReturnsLocalFallbackPath()
    {
        using var client = new HttpClient(new StubHandler(HttpStatusCode.TooManyRequests));
        var provider = new BackendCoachProxyProvider(client, "https://proxy.invalid/v1/coach/generate", "https://proxy.invalid/health", "runtime-token");

        var result = await provider.GenerateAsync(Request());

        Assert.Equal(ExternalCoachError.RateLimited, result.Error);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task HealthReportsVerifiedFreeAvailability()
    {
        using var client = new HttpClient(new AvailabilityHandler());
        var provider = new BackendCoachProxyProvider(client, "https://proxy.invalid/v1/coach/generate", "https://proxy.invalid/health", "session-token");

        var result = await provider.GetAvailabilityAsync();

        Assert.Equal(ExternalCoachAvailabilityState.AvailableFree, result.State);
        Assert.True(result.IsFree);
        Assert.True(result.BillingDisabled);
        Assert.False(result.HasPaidFallback);
    }

    private static ExternalCoachRequest Request()
        => new(new ApprovedCoachContext(5, 40, "delay", null, Array.Empty<string>()), TimeSpan.FromSeconds(5));

    private sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public StubHandler() : this(HttpStatusCode.OK) { }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status)
            {
                Content = JsonContent.Create(new
                {
                    succeeded = true,
                    error = ExternalCoachError.None,
                    text = "A short supportive suggestion.",
                    safetyResult = ExternalCoachSafetyResult.Allowed,
                }),
            });
    }

    private sealed class AvailabilityHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    state = ExternalCoachAvailabilityState.AvailableFree,
                    isFree = true,
                    billingDisabled = true,
                    hasPaidFallback = false,
                }),
            });
    }
}
