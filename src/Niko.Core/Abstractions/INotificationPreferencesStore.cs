// ============================================================================
// Niko.Core — INotificationPreferencesStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد ذخیره/بازیابی ترجیحات اعلان. پیاده‌سازی (SQLite) در
//           Infrastructure است؛ هسته فقط قرارداد را می‌شناسد.
// وابستگی‌ها و لایه: بخش Abstractions در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: ذخیره محلی و آفلاین است. اگر هیچ ترجیحی ذخیره نشده باشد،
//           پیش‌فرض امن (غیرفعال) برمی‌گردد.
// ============================================================================

using Niko.Core.Domain.Notifications;

namespace Niko.Core.Abstractions;

/// <summary>
/// قرارداد ذخیره‌سازی ترجیحات اعلان.
/// </summary>
public interface INotificationPreferencesStore
{
    /// <summary>بازیابی ترجیحات؛ اگر ذخیره نشده باشد null برمی‌گرداند.</summary>
    Task<NotificationPreferences?> GetAsync(CancellationToken ct = default);

    /// <summary>ذخیره یا به‌روزرسانی ترجیحات.</summary>
    Task SaveAsync(NotificationPreferences preferences, CancellationToken ct = default);
}
