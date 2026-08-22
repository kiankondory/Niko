// ============================================================================
// نام فایل: Program.cs
// مسئولیت: راه‌اندازی endpoint امن proxy و dependency injection backend.
// وابستگی‌ها و لایه: Composition root در Backend؛ به Core policy و Gemini adapter متصل است.
// نکات تغییر و قیود: احراز هویت، محدودیت اندازه/نرخ و secret redaction فعال‌اند؛ billing یا paid fallback وجود ندارد.
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Niko.CoachProxy.Configuration;
using Niko.CoachProxy.Contracts;
using Niko.CoachProxy.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow);
var gemini = new GeminiOptions
{
    ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty,
    Model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? string.Empty,
    BaseUrl = Environment.GetEnvironmentVariable("GEMINI_API_BASE_URL") ?? "https://generativelanguage.googleapis.com/v1beta",
    TimeoutSeconds = ParsePositive("GEMINI_TIMEOUT_SECONDS", 8, 30),
    MaxResponseCharacters = 500,
    BillingEnabled = ParseBool("GEMINI_BILLING_ENABLED"),
    FreeQuotaAvailable = ParseBool("GEMINI_FREE_QUOTA_AVAILABLE"),
    ProviderHealthy = ParseBool("GEMINI_PROVIDER_HEALTHY"),
    ProviderReportsPaidAccess = ParseBool("GEMINI_PROVIDER_PAID_ACCESS"),
    PaidFallbackConfigured = ParseBool("GEMINI_PAID_FALLBACK_CONFIGURED") != false,
};
var sessionSecret = Environment.GetEnvironmentVariable("COACH_PROXY_SESSION_SECRET") ?? string.Empty;
var perMinute = ParsePositive("COACH_PROXY_RPM_LIMIT", 3, 30);
var perDay = ParsePositive("COACH_PROXY_DAILY_LIMIT", 50, 500);

builder.WebHost.UseKestrel(options => options.Limits.MaxRequestBodySize = 16 * 1024);
builder.Services.AddSingleton(gemini);
builder.Services.AddSingleton<GeminiOptionsAccessor>();
builder.Services.AddSingleton(new SessionTokenValidator(sessionSecret));
builder.Services.AddSingleton<RequestBudget>();
builder.Services.AddHttpClient<GeminiApiClient>();
builder.Services.AddSingleton<CoachProxyService>();

var app = builder.Build();
app.MapGet("/health", (HttpContext http, SessionTokenValidator validator) =>
{
    if (!validator.IsValid(http.Request.Headers["X-Coach-Session"].ToString()))
    {
        return Results.Json(new { state = Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.AuthenticationRequired, isFree = false, billingDisabled = false, hasPaidFallback = true }, statusCode: StatusCodes.Status401Unauthorized);
    }

    var state = gemini.IsConfigured
        ? Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.AvailableFree
        : string.IsNullOrWhiteSpace(gemini.ApiKey)
            ? Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.NotConfigured
            : gemini.BillingEnabled != false || gemini.ProviderReportsPaidAccess != false || gemini.PaidFallbackConfigured
                ? Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.DisabledByPolicy
                : gemini.FreeQuotaAvailable != true
                    ? Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.FreeQuotaUnavailable
                    : Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.Unavailable;
    return Results.Json(new { state, isFree = state == Niko.Core.Domain.Coach.ExternalCoachAvailabilityState.AvailableFree, billingDisabled = gemini.BillingEnabled == false, hasPaidFallback = gemini.PaidFallbackConfigured });
});
app.MapPost("/v1/coach/generate", async (HttpContext http, CoachProxyRequest request, CoachProxyService service, RequestBudget budget, CancellationToken ct) =>
{
    var supplied = http.Request.Headers["X-Coach-Session"].ToString();
    if (!new SessionTokenValidator(sessionSecret).IsValid(supplied))
    {
        return Results.Unauthorized();
    }

    if (!gemini.IsConfigured)
    {
        return Results.Json(new CoachProxyResponse(false, Niko.Core.Domain.Coach.ExternalCoachError.Unavailable, null, Niko.Core.Domain.Coach.ExternalCoachSafetyResult.Rejected), statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var clientKey = supplied;
    if (!budget.TryAcquire(clientKey, perMinute, perDay, DateTimeOffset.UtcNow))
    {
        return Results.Json(new CoachProxyResponse(false, Niko.Core.Domain.Coach.ExternalCoachError.RateLimited, null, Niko.Core.Domain.Coach.ExternalCoachSafetyResult.Rejected), statusCode: StatusCodes.Status429TooManyRequests);
    }

    var result = await service.GenerateAsync(request, ct);
    return Results.Json(result, statusCode: result.Error == Niko.Core.Domain.Coach.ExternalCoachError.RateLimited ? StatusCodes.Status429TooManyRequests : StatusCodes.Status200OK);
});

app.Run();

static int ParsePositive(string name, int fallback, int max)
    => int.TryParse(Environment.GetEnvironmentVariable(name), out var value) && value > 0
        ? Math.Min(value, max)
        : fallback;

static bool? ParseBool(string name)
    => bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : null;

public partial class Program;
