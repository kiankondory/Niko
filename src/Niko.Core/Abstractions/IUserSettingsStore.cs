// ============================================================================
// Niko.Core — IUserSettingsStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد ذخیره/بازیابی پروفایل کاربر. هسته فقط این قرارداد را می‌شناسد؛
//           پیاده‌سازی (SQLite یا مشابه) در Infrastructure/App قرار دارد.
// وابستگی‌ها و لایه: بخش Abstractions در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: محلی و آفلاین است. پروفایل ممکن است ناقص باشد (فیلدهای
//           اختیاری) و ذخیره باید به‌صورت اتمیک انجام شود.
// ============================================================================

using Niko.Core.Domain;

namespace Niko.Core.Abstractions;

/// <summary>
/// قرارداد ذخیره‌سازی پروفایل کاربر.
/// </summary>
public interface IUserSettingsStore
{
    /// <summary>بازیابی پروفایل؛ اگر هیچ پروفایلی ذخیره نشده باشد null برمی‌گرداند.</summary>
    Task<UserProfile?> GetAsync(CancellationToken ct = default);

    /// <summary>ذخیره یا به‌روزرسانی پروفایل.</summary>
    Task SaveAsync(UserProfile profile, CancellationToken ct = default);
}
