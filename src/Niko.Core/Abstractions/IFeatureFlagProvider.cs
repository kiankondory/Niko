// نام فایل: IFeatureFlagProvider.cs
// مسئولیت: قرارداد خواندن وضعیت feature flagها برای adapterهای ارائه.
// وابستگی‌ها و لایه: قرارداد Core؛ پیاده‌سازی runtime در MAUI قرار می‌گیرد.
// نکات تغییر و قیود: provider نباید داده کاربر، SQLite یا شبکه را مصرف کند و در ابهام باید خاموش عمل کند.

namespace Niko.Core.Abstractions;

public interface IFeatureFlagProvider
{
    bool IsEnabled(FeatureFlag featureFlag);
}
