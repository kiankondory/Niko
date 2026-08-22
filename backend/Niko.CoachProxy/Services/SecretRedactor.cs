// ============================================================================
// نام فایل: SecretRedactor.cs
// مسئولیت: جلوگیری از ورود مقدارهای حساس به log یا exception متن.
// وابستگی‌ها و لایه: Service در Backend؛ توسط endpoint و client استفاده می‌شود.
// نکات تغییر و قیود: این کد هرگز مقدار secret واقعی را تولید یا ثبت نمی‌کند.
// ============================================================================

namespace Niko.CoachProxy.Services;

public static class SecretRedactor
{
    public static string SafeError(Exception exception)
        => exception is OperationCanceledException
            ? "cancelled"
            : exception.GetType().Name;
}
