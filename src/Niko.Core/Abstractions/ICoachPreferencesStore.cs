// ============================================================================
// Niko.Core — ICoachPreferencesStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد ذخیره و حذف ترجیحات مربی بدون وابستگی به SQLite یا پلتفرم.
// وابستگی‌ها و لایه: Abstractions در Core؛ Infrastructure آن را با ذخیره‌سازی محلی پیاده می‌کند.
// نکات تغییر و قیود: مقدار تهی باید به پیش‌فرض خاموش تفسیر شود؛ هیچ دادهٔ مربی بیرونی ذخیره نمی‌شود.
// ============================================================================

using Niko.Core.Domain.Coach;

namespace Niko.Core.Abstractions;

public interface ICoachPreferencesStore
{
    Task<CoachPreferences?> GetAsync(CancellationToken ct = default);
    Task SaveAsync(CoachPreferences preferences, CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}
