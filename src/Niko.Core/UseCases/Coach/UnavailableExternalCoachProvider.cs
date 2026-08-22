// ============================================================================
// Niko.Core — UnavailableExternalCoachProvider.cs
// ----------------------------------------------------------------------------
// مسئولیت: adapter پیش‌فرضی که provider خارجی را صریحاً unavailable نگه می‌دارد.
// وابستگی‌ها و لایه: UseCases/Coach در Core؛ بدون شبکه، SDK، secret یا endpoint.
// نکات تغییر و قیود: تا زمان انتخاب و بررسی provider واقعی هیچ درخواست خارجی اجرا
//           نمی‌شود؛ fallback توسط privacy gateway مدیریت می‌شود.
// ============================================================================

using Niko.Core.Abstractions;
using Niko.Core.Domain.Coach;

namespace Niko.Core.UseCases.Coach;

public sealed class UnavailableExternalCoachProvider : IExternalCoachProvider
{
    public Task<ExternalCoachAvailability> GetAvailabilityAsync(CancellationToken ct = default)
        => Task.FromResult(ExternalCoachAvailability.FailClosed(ExternalCoachAvailabilityState.NotConfigured));

    public Task<ExternalCoachResult> GenerateAsync(
        ExternalCoachRequest request,
        CancellationToken ct = default)
        => Task.FromResult(ExternalCoachResult.Failure(
            ExternalCoachError.Unavailable,
            CoachProviderResult.Failure(CoachProviderError.Unavailable)));
}
