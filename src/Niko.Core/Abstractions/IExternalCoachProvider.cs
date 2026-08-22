// ============================================================================
// Niko.Core — IExternalCoachProvider.cs
// ----------------------------------------------------------------------------
// مسئولیت: قرارداد provider خارجی مربی بدون وابستگی به SDK، شبکه یا vendor.
// وابستگی‌ها و لایه: Abstractions در Core؛ فقط قراردادهای Domain/Coach را مصرف می‌کند.
// نکات تغییر و قیود: provider فقط زمینهٔ تأییدشده را می‌گیرد؛ پیاده‌سازی واقعی،
//           credential و اتصال شبکه در این مرحله عمداً وجود ندارد.
// ============================================================================

using Niko.Core.Domain.Coach;

namespace Niko.Core.Abstractions;

public interface IExternalCoachProvider
{
    Task<ExternalCoachAvailability> GetAvailabilityAsync(CancellationToken ct = default);

    Task<ExternalCoachResult> GenerateAsync(
        ExternalCoachRequest request,
        CancellationToken ct = default);
}
