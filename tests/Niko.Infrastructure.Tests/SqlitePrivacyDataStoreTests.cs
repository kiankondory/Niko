// ============================================================================
// Niko.Infrastructure.Tests — SqlitePrivacyDataStoreTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون export و حذف تراکنشی داده‌های SQLite در فایل موقت.
// وابستگی‌ها و لایه: تست Infrastructure → SqlitePrivacyDataStore/SqliteStore/UserSettingsStore.
// نکات تغییر و قیود: هیچ پایگاه‌دادهٔ دستگاه یا دادهٔ واقعی استفاده نمی‌شود.
// ============================================================================

using System.Text.Json;
using Niko.Core.Domain;
using Niko.Core.Events;
using Niko.Infrastructure.Persistence;

namespace Niko.Infrastructure.Tests;

public sealed class SqlitePrivacyDataStoreTests
{
    [Fact]
    public async Task ExportThenErase_ExportsLocalRowsAndClearsUserDataWithoutDeletingSchema()
    {
        var path = Path.Combine(Path.GetTempPath(), $"niko_privacy_{Guid.NewGuid():N}.db");
        var events = new SqliteStore(path);
        var settings = new UserSettingsStore(path);
        await events.SaveEventAsync(new LogEvent("privacy-event", DateTimeOffset.UtcNow, EventSource.Mobile, EventType.Smoked, SyncStatus.Pending));
        await settings.SaveAsync(new UserProfile { DisplayName = "Local profile", PreferredLocale = "fa" });
        var privacy = new SqlitePrivacyDataStore(path);

        var json = await privacy.ExportJsonAsync();
        using var document = JsonDocument.Parse(json);
        Assert.Equal("niko-local-export-v1", document.RootElement.GetProperty("format").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("data").GetProperty("events").GetArrayLength());
        Assert.Equal(1, document.RootElement.GetProperty("data").GetProperty("user_profile").GetArrayLength());

        await privacy.EraseAllAsync();

        Assert.Empty(await events.GetEventsAsync());
        Assert.Null(await settings.GetAsync());
        var after = await privacy.ExportJsonAsync();
        using var afterDocument = JsonDocument.Parse(after);
        Assert.Equal(0, afterDocument.RootElement.GetProperty("data").GetProperty("events").GetArrayLength());
    }
}
