// ============================================================================
// Niko.Core.Tests — RecoveryCalculatorTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های محاسبهٔ بهبود بدن: صفر، مرحلهٔ اول، میانی، مرزهای زمانی،
//           دورهٔ طولانی و دادهٔ ناقص.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و RecoveryCalculator را استفاده می‌کند.
// نکات تغییر و قیود: تست‌ها قطعی‌اند و «اکنون» را صریح می‌گیرند؛ به شبکه وابسته نیستند.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Domain.Recovery;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public class RecoveryCalculatorTests
{
    private static readonly DateTimeOffset Now = new(2024, 3, 1, 12, 0, 0, TimeSpan.Zero);

    private static UserProfile Quit(DateTimeOffset quit) => new() { QuitDateUtc = quit };

    private static LogEvent Resist(DateTimeOffset at) =>
        new(Guid.NewGuid().ToString("N"), at, EventSource.Mobile, EventType.Resisted, SyncStatus.Pending);

    [Fact]
    public void NoData_ReturnsStage0NoProgressNoData()
    {
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), null, Now);

        Assert.Equal(RecoveryStage.Stage0, result.Stage);
        Assert.Equal(0, result.ProgressPercent);
        Assert.Equal(TimeSpan.Zero, result.SmokeFreeTime);
        Assert.False(result.HasSufficientData);
    }

    [Fact]
    public void FirstStage_JustOverOneDay_SmokeFree()
    {
        // ترک ۱٫۵ روز پیش → مرحلهٔ ۱ (۱ تا ۳ روز).
        var profile = Quit(Now - TimeSpan.FromDays(1.5));
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), profile, Now);

        Assert.Equal(RecoveryStage.Stage1, result.Stage);
        Assert.True(result.HasSufficientData);
    }

    [Fact]
    public void IntermediateStage_AtTenDays_IsStage3()
    {
        var profile = Quit(Now - TimeSpan.FromDays(10));
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), profile, Now);

        Assert.Equal(RecoveryStage.Stage3, result.Stage); // ۷ تا ۱۴ روز
        Assert.True(result.ProgressPercent is > 0 and < 100);
    }

    [Fact]
    public void Boundary_ExactlySevenDays_IsStage3()
    {
        // دقیقاً ۷ روز: بازهٔ Stage2 [۳،۷) است؛ در ۷ وارد Stage3 (۷ تا ۱۴) می‌شویم.
        var profile = Quit(Now - TimeSpan.FromDays(7));
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), profile, Now);

        Assert.Equal(RecoveryStage.Stage3, result.Stage);
    }

    [Fact]
    public void Boundary_JustUnderSevenDays_IsStage2()
    {
        var profile = Quit(Now - TimeSpan.FromDays(6.99));
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), profile, Now);

        Assert.Equal(RecoveryStage.Stage2, result.Stage); // ۳ تا ۷ روز
    }

    [Fact]
    public void LongSmokeFree_Over180Days_IsLastStageFull()
    {
        var profile = Quit(Now - TimeSpan.FromDays(400));
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), profile, Now);

        Assert.Equal(RecoveryStage.Stage7, result.Stage);
        Assert.Equal(100, result.ProgressPercent);
    }

    [Fact]
    public void MissingProfile_FallsBackToLastResist()
    {
        // بدون تاریخ ترک، اما آخرین مقاومت ۵ روز پیش.
        var events = new[] { Resist(Now - TimeSpan.FromDays(5)) };
        var result = RecoveryCalculator.Calculate(events, null, Now);

        Assert.Equal(RecoveryStage.Stage2, result.Stage); // ۳ تا ۷ روز
        Assert.True(result.HasSufficientData);
    }

    [Fact]
    public void MissingProfile_AndNoResist_IsStage0NoData()
    {
        var events = new[] { new LogEvent(Guid.NewGuid().ToString("N"), Now - TimeSpan.FromDays(1), EventSource.Mobile, EventType.Smoked, SyncStatus.Pending) };
        var result = RecoveryCalculator.Calculate(events, null, Now);

        Assert.Equal(RecoveryStage.Stage0, result.Stage);
        Assert.False(result.HasSufficientData);
    }

    [Fact]
    public void ProgressPercent_WithinStage_IsBetweenZeroAndHundred()
    {
        var profile = Quit(Now - TimeSpan.FromDays(20));
        var result = RecoveryCalculator.Calculate(Array.Empty<LogEvent>(), profile, Now);

        // مرحلهٔ ۴: ۱۴ تا ۳۰ روز؛ در روز ۲۰ → ۶/۱۶ ≈ ۳۷٫۵٪
        Assert.Equal(RecoveryStage.Stage4, result.Stage);
        Assert.InRange(result.ProgressPercent, 0, 100);
    }
}
