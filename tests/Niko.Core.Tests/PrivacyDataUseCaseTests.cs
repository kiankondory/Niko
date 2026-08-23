// ============================================================================
// Niko.Core.Tests — PrivacyDataUseCaseTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون مسیر Core برای export و حذف داده بدون UI یا SQLite واقعی.
// وابستگی‌ها و لایه: تست Core → PrivacyDataUseCase/IPrivacyDataStore.
// نکات تغییر و قیود: تست قطعی است و هیچ دادهٔ کاربر یا شبکه‌ای ندارد.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.UseCases.Privacy;

namespace Niko.Core.Tests;

public sealed class PrivacyDataUseCaseTests
{
    [Fact]
    public async Task Export_DelegatesToTheLocalPrivacyStore()
    {
        var store = new FakePrivacyDataStore { Export = "{\"format\":\"niko-local-export-v1\"}" };
        var useCase = new PrivacyDataUseCase(store);

        var result = await useCase.ExportJsonAsync();

        Assert.Equal(store.Export, result);
        Assert.True(store.ExportRequested);
    }

    [Fact]
    public async Task Erase_DelegatesToTheLocalPrivacyStore()
    {
        var store = new FakePrivacyDataStore();
        var useCase = new PrivacyDataUseCase(store);

        await useCase.EraseAllAsync();

        Assert.True(store.EraseRequested);
    }

    private sealed class FakePrivacyDataStore : IPrivacyDataStore
    {
        public string Export { get; init; } = "{}";
        public bool ExportRequested { get; private set; }
        public bool EraseRequested { get; private set; }
        public Task<string> ExportJsonAsync(CancellationToken ct = default) { ExportRequested = true; return Task.FromResult(Export); }
        public Task EraseAllAsync(CancellationToken ct = default) { EraseRequested = true; return Task.CompletedTask; }
    }
}
