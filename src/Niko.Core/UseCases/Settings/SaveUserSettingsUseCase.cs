// ============================================================================
// Niko.Core — SaveUserSettingsUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: مورد کاربرد ذخیرهٔ تنظیمات کاربر. پروفایل را با قواعد دامنه اعتبارسنجی
//           می‌کند و در صورت معتبر بودن، آن را از طریق IUserSettingsStore به‌صورت
//           محلی ذخیره می‌کند. بارگذاری پروفایل فعلی را نیز فراهم می‌کند.
// وابستگی‌ها و لایه: UseCases/Settings → Abstractions (IUserSettingsStore, IClock)
//           + Domain (UserProfile, UserSettingsValidation).
// نکات تغییر و قیود: تمام قواعد اعتبارسنجی در Core است. ذخیره اتمیک و آفلاین است.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain;
using Niko.Core.Domain.Localization;
using Niko.Core.Domain.Settings;

namespace Niko.Core.UseCases.Settings;

/// <summary>
/// مورد کاربرد ذخیره/بارگذاری تنظیمات کاربر.
/// </summary>
public sealed class SaveUserSettingsUseCase
{
    private readonly IUserSettingsStore _store;
    private readonly IClock _clock;

    public SaveUserSettingsUseCase(IUserSettingsStore store, IClock clock)
    {
        _store = store;
        _clock = clock;
    }

    /// <summary>بارگذاری پروفایل فعلی؛ اگر ذخیره نشده باشد null.</summary>
    public async Task<UserProfile?> LoadAsync(CancellationToken ct = default)
        => await _store.GetAsync(ct).ConfigureAwait(false);

    /// <summary>اعتبارسنجی و ذخیرهٔ پروفایل.</summary>
    public async Task<SaveUserSettingsResult> SaveAsync(
        UserProfile profile,
        CancellationToken ct = default)
    {
        var validation = UserSettingsValidation.Validate(profile, _clock.UtcNow);

        if (validation != UserSettingsValidationResult.Valid)
        {
            return new SaveUserSettingsResult(false, validation);
        }

        await _store.SaveAsync(profile, ct).ConfigureAwait(false);

        return new SaveUserSettingsResult(true, SavedProfile: profile);
    }

    /// <summary>ذخیرهٔ locale بدون اجبار کاربر به تکمیل سایر فیلدهای پروفایل.</summary>
    public async Task<bool> SavePreferredLocaleAsync(string locale, CancellationToken ct = default)
    {
        if (!SupportedLocales.IsConfigured(locale))
        {
            return false;
        }

        var current = await _store.GetAsync(ct).ConfigureAwait(false) ?? new UserProfile();
        await _store.SaveAsync(current with { PreferredLocale = locale }, ct).ConfigureAwait(false);
        return true;
    }
}
