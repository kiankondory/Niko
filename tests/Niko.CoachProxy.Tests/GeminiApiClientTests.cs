// ============================================================================
// نام فایل: GeminiApiClientTests.cs
// مسئولیت: آزمون fake Gemini client برای پاسخ معتبر، JSON خراب، timeout، 429 و نبود key.
// وابستگی‌ها و لایه: Backend.Tests → GeminiApiClient؛ بدون API key یا network واقعی.
// نکات تغییر و قیود: fake handler فقط payload و header redaction را بررسی می‌کند.
// ============================================================================

using System.Net;
using System.Text;
using Niko.CoachProxy.Configuration;
using Niko.CoachProxy.Contracts;
using Niko.CoachProxy.Services;
using Niko.Core.Domain.Coach;

namespace Niko.CoachProxy.Tests;

public sealed class GeminiApiClientTests
{
    [Fact]
    public void PaidModelOrBillingConfigurationIsNeverConfigured()
    {
        Assert.False(new GeminiOptions { ApiKey = "key", Model = "gemini-3.1-pro-preview" }.IsConfigured);
        Assert.False(new GeminiOptions { ApiKey = "key", Model = "gemini-3.5-flash-lite", BillingEnabled = true }.IsConfigured);
        Assert.False(new GeminiOptions { ApiKey = "key", Model = "gemini-3.5-flash-lite", BillingEnabled = false }.IsConfigured);
        Assert.False(new GeminiOptions { ApiKey = "key", Model = "gemini-3.5-flash-lite", BillingEnabled = false, FreeQuotaAvailable = true, ProviderHealthy = true, ProviderReportsPaidAccess = true, PaidFallbackConfigured = false }.IsConfigured);
    }

    [Fact]
    public async Task ValidResponseIsMappedWithoutLoggingSecret()
    {
        var handler = new RecordingHandler("""{"candidates":[{"content":{"parts":[{"text":"Try a short pause."}]},"finishReason":"STOP"}]}""");
        var options = Options("test-key");
        var client = new GeminiApiClient(new HttpClient(handler), options);

        var result = await client.GenerateAsync(Request(), CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal("Try a short pause.", result.Text);
        Assert.DoesNotContain("test-key", handler.Body);
        Assert.Equal("test-key", handler.ApiKey);
        Assert.DoesNotContain("raw-note", handler.Body);
    }

    [Fact]
    public async Task MissingKeyIsUnavailable()
    {
        var client = new GeminiApiClient(new HttpClient(new RecordingHandler("{}")), Options(string.Empty));

        var result = await client.GenerateAsync(Request(), CancellationToken.None);

        Assert.Equal(ExternalCoachError.Unavailable, result.Error);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, ExternalCoachError.RateLimited)]
    [InlineData(HttpStatusCode.BadGateway, ExternalCoachError.ProviderFailure)]
    public async Task ProviderStatusIsMapped(HttpStatusCode status, ExternalCoachError expected)
    {
        var client = new GeminiApiClient(new HttpClient(new StatusHandler(status)), Options("test-key"));

        var result = await client.GenerateAsync(Request(), CancellationToken.None);

        Assert.Equal(expected, result.Error);
    }

    [Fact]
    public async Task MalformedResponseIsFailure()
    {
        var client = new GeminiApiClient(new HttpClient(new RecordingHandler("not-json")), Options("test-key"));

        var result = await client.GenerateAsync(Request(), CancellationToken.None);

        Assert.Equal(ExternalCoachError.ProviderFailure, result.Error);
    }

    [Fact]
    public async Task TimeoutIsMapped()
    {
        var options = Options("test-key");
        options = new GeminiOptions
        {
            ApiKey = options.ApiKey,
            Model = options.Model,
            BaseUrl = options.BaseUrl,
            TimeoutSeconds = 1,
            MaxResponseCharacters = options.MaxResponseCharacters,
            BillingEnabled = false,
            FreeQuotaAvailable = true,
            ProviderHealthy = true,
            ProviderReportsPaidAccess = false,
            PaidFallbackConfigured = false,
        };
        var client = new GeminiApiClient(new HttpClient(new DelayHandler()), options);

        var result = await client.GenerateAsync(Request(), CancellationToken.None);

        Assert.Equal(ExternalCoachError.Timeout, result.Error);
    }

    private static GeminiOptions Options(string key)
        => new()
        {
            ApiKey = key,
            Model = "gemini-3.5-flash-lite",
            TimeoutSeconds = 2,
            BillingEnabled = false,
            FreeQuotaAvailable = true,
            ProviderHealthy = true,
            ProviderReportsPaidAccess = false,
            PaidFallbackConfigured = false,
        };

    private static CoachProxyRequest Request()
        => new(new ApprovedCoachContext(4, 20, "delay", null, Array.Empty<string>()));

    private sealed class RecordingHandler(string response) : HttpMessageHandler
    {
        public string Body { get; private set; } = string.Empty;
        public string ApiKey { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            ApiKey = request.Headers.GetValues("x-goog-api-key").Single();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
            };
        }
    }

    private sealed class StatusHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status));
    }

    private sealed class DelayHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
