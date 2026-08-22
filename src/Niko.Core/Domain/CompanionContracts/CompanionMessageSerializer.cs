// ============================================================================
// Niko.Core — CompanionMessageSerializer.cs
// ----------------------------------------------------------------------------
// مسئولیت: سریال/دی‌سریال کردن پیام‌های قرارداد ابزارک/ساعت (JSON) و بررسی نسخهٔ
//           قرارداد. روی پیام ناقص/نامعتبر به‌صورت امن رفتار می‌کند.
// وابستگی‌ها و لایه: بخش Domain/CompanionContracts در Core؛ فقط System.Text.Json.
// نکات تغییر و قیود: نسخهٔ فعلی ۱ است؛ نسخه‌های بالاتر «پشتیبانی‌نشده» تلقی می‌شوند.
// ============================================================================

using System.Text.Json;

namespace Niko.Core.Domain.CompanionContracts;

/// <summary>
/// سریال‌کنندهٔ پیام قرارداد ابزارک/ساعت.
/// </summary>
public static class CompanionMessageSerializer
{
    /// <summary>نسخهٔ فعلی قرارداد.</summary>
    public const int CurrentContractVersion = 1;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>سریال کردن پیام به JSON.</summary>
    public static string Serialize<T>(T payload)
        => JsonSerializer.Serialize(payload, Options);

    /// <summary>دی‌سریال کردن پیام از JSON؛ در صورت نامعتبر بودن null برمی‌گرداند.</summary>
    public static CompanionMessage? DeserializeMessage(string json)
    {
        try
        {
            var message = JsonSerializer.Deserialize<CompanionMessage>(json, Options);
            if (message is null)
            {
                return null;
            }

            // نسخهٔ پیش‌فرض اگر ذکر نشده باشد ۱ است.
            if (message.ContractVersion == 0)
            {
                message = message with { ContractVersion = CurrentContractVersion };
            }

            return message;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>دی‌سریال کردن محتوای (Payload) یک نوع خاص.</summary>
    public static T? DeserializePayload<T>(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload, Options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>آیا نسخهٔ قرارداد پشتیبانی می‌شود؟</summary>
    public static bool IsVersionSupported(int version)
        => version == CurrentContractVersion;
}
