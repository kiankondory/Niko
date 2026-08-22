// ============================================================================
// Niko.Infrastructure.Tests — UserSettingsStoreTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های یکپارچهٔ ذخیره/بازیابی پروفایل کاربر در SQLite، از جمله
//           پروفایل ناقص و پایداری بین دو نمونهٔ ذخیره‌ساز.
// وابستگی‌ها و لایه: لایهٔ تست؛ Infrastructure و Core را استفاده می‌کند.
// نکات تغییر و قیود: از پایگاه‌دادهٔ موقت هر اجرا استفاده می‌کند؛ بدون شبکه.
// ============================================================================

using Niko.Core.Domain;
using Niko.Infrastructure.Persistence;

namespace Niko.Infrastructure.Tests;

public class UserSettingsStoreTests
{
    private static string NewTempPath()
        => Path.Combine(Path.GetTempPath(), $"niko_{Guid.NewGuid():N}.db");

    [Fact]
    public async Task Get_WithNoSavedProfile_ReturnsNull()
    {
        var store = new UserSettingsStore(NewTempPath());
        var profile = await store.GetAsync();

        Assert.Null(profile);
    }

    [Fact]
    public async Task SaveThenGet_ReturnsSameProfile()
    {
        var store = new UserSettingsStore(NewTempPath());
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0.5m,
            CurrencyCode = "EUR",
            PreferredLocale = "fa-IR",
            DisplayName = "Niko",
            AvatarId = "niko-leaf",
        };

        await store.SaveAsync(profile);
        var loaded = await store.GetAsync();

        Assert.NotNull(loaded);
        Assert.Equal(profile.QuitDateUtc, loaded.QuitDateUtc);
        Assert.Equal(10, loaded.CigarettesPerDay);
        Assert.Equal(0.5m, loaded.PricePerCigarette);
        Assert.Equal("EUR", loaded.CurrencyCode);
        Assert.Equal("fa-IR", loaded.PreferredLocale);
        Assert.Equal("Niko", loaded.DisplayName);
        Assert.Equal("niko-leaf", loaded.AvatarId);
    }

    [Fact]
    public async Task Save_PartialProfile_RoundTripsNulls()
    {
        var store = new UserSettingsStore(NewTempPath());
        var profile = new UserProfile { CurrencyCode = "USD" };

        await store.SaveAsync(profile);
        var loaded = await store.GetAsync();

        Assert.NotNull(loaded);
        Assert.Null(loaded.QuitDateUtc);
        Assert.Null(loaded.CigarettesPerDay);
        Assert.Null(loaded.PricePerCigarette);
    }

    [Fact]
    public async Task SaveThenUpdate_OverwritesExistingRow()
    {
        var store = new UserSettingsStore(NewTempPath());
        await store.SaveAsync(new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
        });
        await store.SaveAsync(new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 5,
        });

        var loaded = await store.GetAsync();

        Assert.Equal(new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero), loaded!.QuitDateUtc);
        Assert.Equal(5, loaded.CigarettesPerDay);
    }

    [Fact]
    public async Task PersistsAcrossNewStoreInstance_SameFile()
    {
        var path = NewTempPath();
        var store1 = new UserSettingsStore(path);
        await store1.SaveAsync(new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 8,
            PricePerCigarette = 0.25m,
        });

        var store2 = new UserSettingsStore(path);
        var loaded = await store2.GetAsync();

        Assert.Equal(8, loaded!.CigarettesPerDay);
        Assert.Equal(0.25m, loaded.PricePerCigarette);
    }

    [Fact]
    public async Task SaveThenGet_PackBasedPricing_RoundTrips()
    {
        var store = new UserSettingsStore(NewTempPath());
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerPack = 6.5m,
            PackSize = 20,
            CurrencyCode = "EUR",
        };

        await store.SaveAsync(profile);
        var loaded = await store.GetAsync();

        Assert.Equal(6.5m, loaded!.PricePerPack);
        Assert.Equal(20, loaded.PackSize);
        Assert.Equal(0.325m, loaded.EffectivePricePerCigarette);
        Assert.Equal("EUR", loaded.CurrencyCode);
    }

    [Fact]
    public async Task Update_ClearsPerCigaretteAndSetsPack()
    {
        var store = new UserSettingsStore(NewTempPath());
        await store.SaveAsync(new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0.5m,
        });
        await store.SaveAsync(new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerPack = 6m,
            PackSize = 20,
        });

        var loaded = await store.GetAsync();

        Assert.Null(loaded!.PricePerCigarette);
        Assert.Equal(6m, loaded.PricePerPack);
        Assert.Equal(20, loaded.PackSize);
        Assert.Equal(0.3m, loaded.EffectivePricePerCigarette);
    }

    [Fact]
    public async Task SaveThenUpdate_UpdatesLocaleAndAvatar()
    {
        var path = NewTempPath();
        var store = new UserSettingsStore(path);

        await store.SaveAsync(new UserProfile
        {
            PreferredLocale = "en",
            DisplayName = "First",
            AvatarId = "niko-default",
        });
        await store.SaveAsync(new UserProfile
        {
            PreferredLocale = "zh-Hans",
            DisplayName = "Second",
            AvatarId = "niko-wave",
        });

        var reloaded = await new UserSettingsStore(path).GetAsync();

        Assert.Equal("zh-Hans", reloaded!.PreferredLocale);
        Assert.Equal("Second", reloaded.DisplayName);
        Assert.Equal("niko-wave", reloaded.AvatarId);
    }
}
