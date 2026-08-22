// ============================================================================
// Niko.Core.Tests — InMemoryUserSettingsStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی درون‌حافظهٔ IUserSettingsStore برای تست مورد کاربرد داشبورد.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای تست.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain;

namespace Niko.Core.Tests;

/// <summary>
/// ذخیره‌ساز پروفایل درون‌حافظهٔ تستی.
/// </summary>
public sealed class InMemoryUserSettingsStore : IUserSettingsStore
{
    public UserProfile? Profile { get; set; }

    public Task<UserProfile?> GetAsync(CancellationToken ct = default)
        => Task.FromResult(Profile);

    public Task SaveAsync(UserProfile profile, CancellationToken ct = default)
    {
        Profile = profile;
        return Task.CompletedTask;
    }
}
