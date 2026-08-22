// ============================================================================
// نام فایل: CoachProxyServiceTests.cs
// مسئولیت: آزمون redaction، policy خروجی و rejection زمینهٔ خام در proxy.
// وابستگی‌ها و لایه: Backend.Tests → CoachProxyService و Core policy؛ بدون provider واقعی.
// نکات تغییر و قیود: زمینهٔ مجاز aggregate است و خروجی ناامن هرگز عبور نمی‌کند.
// ============================================================================

using Niko.CoachProxy.Configuration;
using Niko.CoachProxy.Contracts;
using Niko.CoachProxy.Services;
using Niko.Core.Domain.Coach;

namespace Niko.CoachProxy.Tests;

public sealed class CoachProxyServiceTests
{
    [Fact]
    public async Task InvalidContextIsRejectedBeforeGemini()
    {
        var handler = new RecordingHandler();
        var service = new CoachProxyService(
            new GeminiApiClient(new HttpClient(handler), ValidOptions()),
            new GeminiOptionsAccessor(ValidOptions()));

        var request = new CoachProxyRequest(new ApprovedCoachContext(null, null, new string('x', 41), null, Array.Empty<string>()));
        var result = await service.GenerateAsync(request, CancellationToken.None);

        Assert.Equal(ExternalCoachError.PayloadTooLarge, result.Error);
        Assert.False(handler.Called);
    }

    [Fact]
    public async Task UnsafeProviderTextIsRejected()
    {
        var handler = new RecordingHandler("diagnosis confirmed");
        var options = ValidOptions();
        var service = new CoachProxyService(new GeminiApiClient(new HttpClient(handler), options), new GeminiOptionsAccessor(options));

        var result = await service.GenerateAsync(new CoachProxyRequest(new ApprovedCoachContext(3, null, null, null, Array.Empty<string>())), CancellationToken.None);

        Assert.Equal(ExternalCoachError.UnsafeOutput, result.Error);
    }

    private sealed class RecordingHandler(string? text = null) : HttpMessageHandler
    {
        public bool Called { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Called = true;
            var value = text ?? "A short supportive suggestion.";
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"candidates\":[{{\"content\":{{\"parts\":[{{\"text\":\"{value}\"}}]}}}}]}}"),
            });
        }
    }

    private static GeminiOptions ValidOptions()
        => new()
        {
            ApiKey = "key",
            Model = "gemini-3.5-flash-lite",
            BillingEnabled = false,
            FreeQuotaAvailable = true,
            ProviderHealthy = true,
            ProviderReportsPaidAccess = false,
            PaidFallbackConfigured = false,
        };
}
