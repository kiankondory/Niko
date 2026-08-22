// نام فایل: FeatureFlagValueParser.cs
// مسئولیت: تفسیر قطعی مقدار متنی feature flag با پیش‌فرض امن.
// وابستگی‌ها و لایه: ابزار مستقل Core؛ توسط adapter runtime و تست‌ها استفاده می‌شود.
// نکات تغییر و قیود: مقدار خالی طبق default برمی‌گردد و مقدار ناشناخته fail-closed است.

namespace Niko.Core.Abstractions;

public static class FeatureFlagValueParser
{
    public static bool Parse(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "on" or "enabled" => true,
            "0" or "false" or "off" or "disabled" => false,
            _ => false,
        };
    }
}
