// ============================================================================
// نام فایل: GeminiOptions.cs
// مسئولیت: تنظیمات backend برای مدل و محدودیت‌های Gemini Free Tier.
// وابستگی‌ها و لایه: Configuration در Backend؛ توسط GeminiApiClient مصرف می‌شود.
// نکات تغییر و قیود: کلید هرگز bind یا log نمی‌شود و مدل پرداختی fallback ندارد.
// ============================================================================

namespace Niko.CoachProxy.Configuration;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://generativelanguage.googleapis.com/v1beta";
    public int TimeoutSeconds { get; init; } = 8;
    public int MaxResponseCharacters { get; init; } = 500;
    public bool? BillingEnabled { get; init; }
    public bool? FreeQuotaAvailable { get; init; }
    public bool? ProviderHealthy { get; init; }
    public bool? ProviderReportsPaidAccess { get; init; }
    public bool PaidFallbackConfigured { get; init; } = true;

    private static readonly string[] FreeTierModels =
    {
        "gemini-3.5-flash-lite",
        "gemini-3.1-flash-lite",
    };

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) &&
        FreeTierModels.Contains(Model, StringComparer.Ordinal) &&
        BillingEnabled == false &&
        FreeQuotaAvailable == true &&
        ProviderHealthy == true &&
        ProviderReportsPaidAccess == false &&
        !PaidFallbackConfigured &&
        Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps &&
        TimeoutSeconds is > 0 and <= 30 &&
        MaxResponseCharacters is > 0 and <= 500;

    public static bool IsFreeTierModel(string model)
        => FreeTierModels.Contains(model, StringComparer.Ordinal);
}
