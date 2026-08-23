// ============================================================================
// Niko.Core — PrivacyDataUseCase.cs
// ----------------------------------------------------------------------------
// مسئولیت: مسیر Core برای export محلی و پاک‌سازی صریح داده‌های Niko.
// وابستگی‌ها و لایه: Core use case → IPrivacyDataStore؛ بدون UI یا پلتفرم.
// نکات تغییر و قیود: مجوز قفل دستگاه در adapter پلتفرم بررسی می‌شود؛ این کلاس شبکه ندارد.
// ============================================================================

using Niko.Core.Abstractions;

namespace Niko.Core.UseCases.Privacy;

public sealed class PrivacyDataUseCase(IPrivacyDataStore store)
{
    public Task<string> ExportJsonAsync(CancellationToken ct = default) => store.ExportJsonAsync(ct);
    public Task EraseAllAsync(CancellationToken ct = default) => store.EraseAllAsync(ct);
}
