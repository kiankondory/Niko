// ============================================================================
// Niko.Core.Tests — UserSettingsValidationTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های اعتبارسنجی تنظیمات کاربر: پروفایل کامل، ناقص، صفر، منفی،
//           قیمت بسته، مقادیر اعشاری، ارز و تاریخ ترک.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و UserSettingsValidation را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و «اکنون» را صریح می‌گیرند.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.Settings;

namespace Niko.Core.Tests;

public class UserSettingsValidationTests
{
    private static readonly DateTimeOffset Now = new(2024, 3, 1, 12, 0, 0, TimeSpan.Zero);

    private static UserProfile Valid() => new()
    {
        QuitDateUtc = Now - TimeSpan.FromDays(5),
        CigarettesPerDay = 10,
        PricePerCigarette = 0.5m,
        CurrencyCode = "USD",
    };

    [Fact]
    public void CompleteValidProfile_IsValid()
    {
        Assert.Equal(UserSettingsValidationResult.Valid,
            UserSettingsValidation.Validate(Valid(), Now));
    }

    [Fact]
    public void MissingQuitDate_IsInvalidQuitDate()
    {
        var profile = Valid() with { QuitDateUtc = null };
        Assert.Equal(UserSettingsValidationResult.InvalidQuitDate,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void ZeroCigarettesPerDay_IsInvalid()
    {
        var profile = Valid() with { CigarettesPerDay = 0 };
        Assert.Equal(UserSettingsValidationResult.InvalidCigarettesPerDay,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void NegativeCigarettesPerDay_IsInvalid()
    {
        var profile = Valid() with { CigarettesPerDay = -3 };
        Assert.Equal(UserSettingsValidationResult.InvalidCigarettesPerDay,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void ZeroPrice_IsInvalid()
    {
        var profile = Valid() with { PricePerCigarette = 0 };
        Assert.Equal(UserSettingsValidationResult.InvalidPrice,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void NegativePackPrice_IsInvalid()
    {
        var profile = Valid() with { PricePerCigarette = null, PricePerPack = -5, PackSize = 20 };
        Assert.Equal(UserSettingsValidationResult.InvalidPrice,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void ZeroPackSize_IsInvalid()
    {
        var profile = Valid() with { PricePerCigarette = null, PricePerPack = 5, PackSize = 0 };
        Assert.Equal(UserSettingsValidationResult.InvalidPackSize,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void MissingAnyPrice_IsMissingPrice()
    {
        var profile = Valid() with { PricePerCigarette = null, PricePerPack = null, PackSize = null };
        Assert.Equal(UserSettingsValidationResult.MissingPrice,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void PackBasedPricing_IsValid_AndEffectivePriceComputed()
    {
        var profile = Valid() with { PricePerCigarette = null, PricePerPack = 6m, PackSize = 20 };
        Assert.Equal(UserSettingsValidationResult.Valid,
            UserSettingsValidation.Validate(profile, Now));
        Assert.Equal(0.3m, profile.EffectivePricePerCigarette);
    }

    [Fact]
    public void FractionalValues_AreValid()
    {
        var profile = Valid() with { CigarettesPerDay = 12, PricePerCigarette = 0.35m };
        Assert.Equal(UserSettingsValidationResult.Valid,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void EmptyCurrency_IsInvalid()
    {
        var profile = Valid() with { CurrencyCode = "  " };
        Assert.Equal(UserSettingsValidationResult.InvalidCurrency,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void FutureQuitDate_IsInvalid()
    {
        var profile = Valid() with { QuitDateUtc = Now + TimeSpan.FromDays(1) };
        Assert.Equal(UserSettingsValidationResult.InvalidQuitDate,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void QuitDateToday_IsValid()
    {
        var profile = Valid() with { QuitDateUtc = Now };
        Assert.Equal(UserSettingsValidationResult.Valid,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void OptionalIdentityFields_AreValid()
    {
        var profile = Valid() with
        {
            DisplayName = "Niko",
            AvatarId = "niko-leaf",
            PreferredLocale = "fa",
        };

        Assert.Equal(UserSettingsValidationResult.Valid,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void DisplayName_OverLimit_IsInvalid()
    {
        var profile = Valid() with { DisplayName = new string('x', 81) };

        Assert.Equal(UserSettingsValidationResult.InvalidDisplayName,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void UnknownAvatar_IsInvalid()
    {
        var profile = Valid() with { AvatarId = "unknown" };

        Assert.Equal(UserSettingsValidationResult.InvalidAvatar,
            UserSettingsValidation.Validate(profile, Now));
    }

    [Fact]
    public void UnconfiguredLocale_IsInvalid()
    {
        var profile = Valid() with { PreferredLocale = "xx" };

        Assert.Equal(UserSettingsValidationResult.InvalidLocale,
            UserSettingsValidation.Validate(profile, Now));
    }
}
