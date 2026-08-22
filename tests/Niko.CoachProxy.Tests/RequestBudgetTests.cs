// ============================================================================
// نام فایل: RequestBudgetTests.cs
// مسئولیت: آزمون محدودیت RPM و daily budget برای جلوگیری از abuse و هزینهٔ ناخواسته.
// وابستگی‌ها و لایه: Backend.Tests → RequestBudget؛ بدون شبکه، زمان واقعی یا billing.
// نکات تغییر و قیود: budget محلی process است و در deployment باید با gateway production تکمیل شود.
// ============================================================================

using Niko.CoachProxy.Services;

namespace Niko.CoachProxy.Tests;

public sealed class RequestBudgetTests
{
    [Fact]
    public void MinuteAndDayLimitsAreEnforced()
    {
        var budget = new RequestBudget();
        var now = new DateTimeOffset(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

        Assert.True(budget.TryAcquire("client", 2, 3, now));
        Assert.True(budget.TryAcquire("client", 2, 3, now));
        Assert.False(budget.TryAcquire("client", 2, 3, now));
        Assert.True(budget.TryAcquire("client", 2, 3, now.AddMinutes(1)));
        Assert.False(budget.TryAcquire("client", 2, 3, now.AddMinutes(1)));
    }
}
