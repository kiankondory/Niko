// ============================================================================
// Niko.App — NoopWidgetRefreshService.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی بی‌عمل بازخوانی ابزارک برای سکوهای بدون ابزارک Android.
// وابستگی‌ها و لایه: آداپتر ارائهٔ MAUI؛ به Core یا ذخیره‌سازی دسترسی ندارد.
// نکات تغییر و قیود: رفتار QuickLog را تغییر نمی‌دهد و فقط قرارداد refresh را
//           برای ترکیب وابستگی چندسکویی تکمیل می‌کند.
// ============================================================================

namespace Niko.Services;

public sealed class NoopWidgetRefreshService : IWidgetRefreshService
{
    public Task RequestRefreshAsync(CancellationToken ct = default)
        => Task.CompletedTask;
}
