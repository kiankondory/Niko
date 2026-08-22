// ============================================================================
// Niko.Core.Tests — SaveUserSettingsUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های مورد کاربرد ذخیرهٔ تنظیمات: اعتبارسنجی و ذخیره/بارگذاری از
//           ذخیره‌ساز محلی.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و تست‌دابل‌ها را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و از FakeClock استفاده می‌کنند.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.Settings;
using Niko.Core.UseCases.Settings;

namespace Niko.Core.Tests;

public class SaveUserSettingsUseCaseTests
{
    private readonly FakeClock _clock;
    private readonly InMemoryUserSettingsStore _store;
    private readonly SaveUserSettingsUseCase _useCase;

    public SaveUserSettingsUseCaseTests()
    {
        _clock = new FakeClock { UtcNow = new DateTimeOffset(2024, 3, 1, 12, 0, 0, TimeSpan.Zero) };
        _store = new InMemoryUserSettingsStore();
        _useCase = new SaveUserSettingsUseCase(_store, _clock);
    }

    private UserProfile Valid() => new()
    {
        QuitDateUtc = _clock.UtcNow - TimeSpan.FromDays(5),
        CigarettesPerDay = 10,
        PricePerCigarette = 0.5m,
        CurrencyCode = "USD",
    };

    [Fact]
    public async Task Save_ValidProfile_PersistsAndReturnsValid()
    {
        var profile = Valid();
        var result = await _useCase.SaveAsync(profile);

        Assert.True(result.IsValid);
        Assert.NotNull(_store.Profile);
    }

    [Fact]
    public async Task Save_InvalidProfile_DoesNotPersist()
    {
        var profile = Valid() with { CigarettesPerDay = 0 };
        var result = await _useCase.SaveAsync(profile);

        Assert.False(result.IsValid);
        Assert.Equal(UserSettingsValidationResult.InvalidCigarettesPerDay, result.Error);
        Assert.Null(_store.Profile);
    }

    [Fact]
    public async Task Save_ThenLoad_ReturnsSavedProfile()
    {
        var profile = Valid() with { PricePerCigarette = null, PricePerPack = 6m, PackSize = 20 };
        await _useCase.SaveAsync(profile);

        var loaded = await _useCase.LoadAsync();

        Assert.NotNull(loaded);
        Assert.Equal(6m, loaded.PricePerPack);
        Assert.Equal(20, loaded.PackSize);
        Assert.Equal(0.3m, loaded.EffectivePricePerCigarette);
    }

    [Fact]
    public async Task Load_WithNoSavedProfile_ReturnsNull()
    {
        var loaded = await _useCase.LoadAsync();
        Assert.Null(loaded);
    }

    [Fact]
    public async Task SavePreferredLocale_PersistsWithoutCompleteProfile()
    {
        var result = await _useCase.SavePreferredLocaleAsync("ar");

        Assert.True(result);
        Assert.Equal("ar", _store.Profile!.PreferredLocale);
    }

    [Fact]
    public async Task SavePreferredLocale_RejectsUnconfiguredLocale()
    {
        var result = await _useCase.SavePreferredLocaleAsync("xx");

        Assert.False(result);
        Assert.Null(_store.Profile);
    }
}
