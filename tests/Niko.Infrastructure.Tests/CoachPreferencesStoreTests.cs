// ============================================================================
// Niko.Infrastructure.Tests — CoachPreferencesStoreTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون ذخیره، به‌روزرسانی، حذف و بازیابی ترجیحات مربی در SQLite.
// وابستگی‌ها و لایه: تست Infrastructure با قراردادهای Core؛ فقط فایل موقت محلی.
// نکات تغییر و قیود: تست‌ها دادهٔ کاربر واقعی ندارند و مهاجرت افزایشی نسخهٔ ۷ را
//           بدون حذف جداول یا داده‌های قبلی بررسی می‌کنند.
// ============================================================================

using Niko.Core.Domain.Coach;
using Niko.Infrastructure.Persistence;

namespace Niko.Infrastructure.Tests;

public sealed class CoachPreferencesStoreTests
{
    [Fact]
    public async Task Get_WithNoSavedPreferences_ReturnsNull()
    {
        var store = new CoachPreferencesStore(NewTempPath());

        Assert.Null(await store.GetAsync());
    }

    [Fact]
    public async Task SaveThenGet_RoundTripsAllFlags()
    {
        var store = new CoachPreferencesStore(NewTempPath());
        var expected = new CoachPreferences
        {
            Enabled = true,
            AllowExternalProvider = true,
            AllowAggregatedProgressContext = true,
            AllowCravingContext = true,
        };

        await store.SaveAsync(expected);

        Assert.Equal(expected, await store.GetAsync());
    }

    [Fact]
    public async Task SaveThenUpdate_OverwritesRow()
    {
        var store = new CoachPreferencesStore(NewTempPath());
        await store.SaveAsync(new CoachPreferences { Enabled = true, AllowCravingContext = true });
        await store.SaveAsync(new CoachPreferences { Enabled = false, AllowAggregatedProgressContext = true });

        Assert.Equal(new CoachPreferences { AllowAggregatedProgressContext = true }, await store.GetAsync());
    }

    [Fact]
    public async Task Clear_RemovesSavedPreferences()
    {
        var store = new CoachPreferencesStore(NewTempPath());
        await store.SaveAsync(new CoachPreferences { Enabled = true });

        await store.ClearAsync();

        Assert.Null(await store.GetAsync());
    }

    [Fact]
    public async Task ExternalProviderConsent_SaveUpdateAndClear_IsPersistedIndependently()
    {
        var store = new CoachPreferencesStore(NewTempPath());
        await store.SaveAsync(new CoachPreferences { Enabled = true, AllowExternalProvider = true });

        Assert.True((await store.GetAsync())!.AllowExternalProvider);

        await store.SaveAsync(new CoachPreferences { Enabled = true, AllowExternalProvider = false });
        Assert.False((await store.GetAsync())!.AllowExternalProvider);

        await store.SaveAsync(new CoachPreferences { Enabled = true, AllowExternalProvider = true });
        await store.ClearAsync();
        Assert.Null(await store.GetAsync());
    }

    private static string NewTempPath()
        => Path.Combine(Path.GetTempPath(), $"niko_coach_{Guid.NewGuid():N}.db");
}
