// نام فایل: EnvironmentFeatureFlagProvider.cs
// مسئولیت: ارائه وضعیت feature flagهای MAUI از تنظیم runtime محیط اجرا.
// وابستگی‌ها و لایه: adapter لایه ارائه → قراردادهای Core؛ بدون SQLite و بدون شبکه.
// نکات تغییر و قیود: Trigger Analysis به‌صورت پیش‌فرض فعال است؛ مقدار نامعتبر یا خاموش‌سازی، UI را fail-closed پنهان می‌کند.

using Niko.Core.Abstractions;

namespace Niko.Services;

public sealed class EnvironmentFeatureFlagProvider : IFeatureFlagProvider
{
    public bool IsEnabled(FeatureFlag featureFlag)
    {
        return featureFlag switch
        {
            FeatureFlag.TriggerAnalysisUi => FeatureFlagValueParser.Parse(
                Environment.GetEnvironmentVariable("NIKO_FEATURE_TRIGGER_ANALYSIS_UI"),
                defaultValue: true),
            _ => false,
        };
    }
}
