// ============================================================================
// Niko.Core.Tests — ProgressCalculatorTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: تست‌های محاسبات داشبورد: شمارش رویدادها، استریک، میل‌استون و
//           صرفه‌جویی تقریبی. تست‌ها قطعی‌اند و تاریخ «امروز» را صریح می‌گیرند.
// وابستگی‌ها و لایه: لایهٔ تست؛ Core و مدل‌های دامنه را استفاده می‌کند.
// نکات تغییر و قیود: بدون شبکه و بدون زمان واقعی.
// ============================================================================

using Niko.Core.Domain;
using Niko.Core.Events;

namespace Niko.Core.Tests;

public class ProgressCalculatorTests
{
    private static readonly DateOnly Today = new(2024, 1, 10);

    private static LogEvent Evt(
        EventType type,
        DateTimeOffset occurredAtUtc)
    {
        return new LogEvent(Guid.NewGuid().ToString("N"), occurredAtUtc, EventSource.Mobile, type, SyncStatus.Pending);
    }

    private static DateTimeOffset Day(int day) =>
        new(2024, 1, day, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Calculate_WithEmptyEvents_ReturnsZeroedSnapshot()
    {
        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), null, Today);

        Assert.Equal(0, snap.TotalSmoked);
        Assert.Equal(0, snap.TotalResisted);
        Assert.Equal(0, snap.TotalCravings);
        Assert.Equal(0, snap.CurrentStreakDays);
        Assert.Equal(0, snap.MilestoneProgressPercent);
        Assert.Null(snap.ApproximateSavings);
    }

    [Fact]
    public void Calculate_CountsEventTypes()
    {
        var events = new[]
        {
            Evt(EventType.Smoked, Day(1)),
            Evt(EventType.Smoked, Day(2)),
            Evt(EventType.Resisted, Day(3)),
            Evt(EventType.Craving, Day(3)),
            Evt(EventType.Craving, Day(4)),
        };

        var snap = ProgressCalculator.Calculate(events, null, Today);

        Assert.Equal(2, snap.TotalSmoked);
        Assert.Equal(1, snap.TotalResisted);
        Assert.Equal(2, snap.TotalCravings);
    }

    [Fact]
    public void Streak_CountsConsecutiveResistedDays()
    {
        var events = new[]
        {
            Evt(EventType.Resisted, Day(8)),
            Evt(EventType.Resisted, Day(9)),
            Evt(EventType.Resisted, Day(10)),
        };

        var snap = ProgressCalculator.Calculate(events, null, Today);

        Assert.Equal(3, snap.CurrentStreakDays);
    }

    [Fact]
    public void Streak_BreaksOnSmokedDay()
    {
        var events = new[]
        {
            Evt(EventType.Resisted, Day(8)),
            Evt(EventType.Resisted, Day(9)),
            Evt(EventType.Smoked, Day(9)),
            Evt(EventType.Resisted, Day(10)),
        };

        var snap = ProgressCalculator.Calculate(events, null, Today);

        // روز ۹ مصرف داشته؛ استریک فقط از ۱۰ ادامه دارد → ۱ روز.
        Assert.Equal(1, snap.CurrentStreakDays);
    }

    [Fact]
    public void Streak_BreaksOnGapBetweenResistedDays()
    {
        var events = new[]
        {
            Evt(EventType.Resisted, Day(7)),
            Evt(EventType.Resisted, Day(8)),
            Evt(EventType.Resisted, Day(10)),
        };

        var snap = ProgressCalculator.Calculate(events, null, Today);

        // روز ۹ بدون مقاومت؛ استریک از ۱۰ → ۱ روز.
        Assert.Equal(1, snap.CurrentStreakDays);
    }

    [Fact]
    public void Milestone_AtSevenDays_TargetsFourteen()
    {
        // استریک ۷ روز: ۱،۳،۷ → فعلی ۷، بعدی ۱۴، درصد = ۰.
        var events = Enumerable.Range(1, 7)
            .Select(d => Evt(EventType.Resisted, Day(d)))
            .ToArray();

        var snap = ProgressCalculator.Calculate(events, null, Today);

        Assert.Equal(7, snap.CurrentStreakDays);
        Assert.Equal(7, snap.CurrentMilestoneDays);
        Assert.Equal(14, snap.NextMilestoneDays);
        Assert.Equal(0, snap.MilestoneProgressPercent);
    }

    [Fact]
    public void Milestone_AtFiveDays_IsHalfwayToSeven()
    {
        // آستانه‌ها: ۱،۳،۷... در روز ۵: فعلی ۳، بعدی ۷، درصد = (۵−۳)/(۷−۳) = ۵۰٪
        var events = Enumerable.Range(1, 5)
            .Select(d => Evt(EventType.Resisted, Day(d)))
            .ToArray();

        var snap = ProgressCalculator.Calculate(events, null, Today);

        Assert.Equal(5, snap.CurrentStreakDays);
        Assert.Equal(3, snap.CurrentMilestoneDays);
        Assert.Equal(7, snap.NextMilestoneDays);
        Assert.Equal(50, snap.MilestoneProgressPercent);
    }

    [Fact]
    public void Savings_WithFullProfile_CalculatesApproximateAmount()
    {
        // ترک ۵ روز پیش، ۱۰ نخ در روز، ۰ نخ واقعی مصرف، قیمت ۰٫۵ → ۵×۱۰×۰٫۵ = ۲۵
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0.5m,
        };
        var events = new[] { Evt(EventType.Resisted, Day(6)) };

        var snap = ProgressCalculator.Calculate(events, profile, Today);

        Assert.Equal(25m, snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_SubtractsActuallySmoked()
    {
        // ترک ۵ روز پیش، ۱۰ نخ در روز، ۲ نخ واقعی مصرف → (۵×۱۰ − ۲)×۰٫۵ = ۲۴
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0.5m,
        };
        var events = new[] { Evt(EventType.Smoked, Day(6)), Evt(EventType.Smoked, Day(7)) };

        var snap = ProgressCalculator.Calculate(events, profile, Today);

        Assert.Equal(24m, snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_WithIncompleteProfile_IsNull()
    {
        var profile = new UserProfile { QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero) };
        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), profile, Today);

        Assert.Null(snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_NoProfile_IsNull()
    {
        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), null, Today);
        Assert.Null(snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_PackBasedPricing_UsesPackPriceDividedByPackSize()
    {
        // قیمت هر بسته ۶، اندازهٔ بسته ۲۰ → ۰٫۳ هر نخ. ترک ۵ روز، ۱۰ نخ/روز → ۵۰×۰٫۳ = ۱۵
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerPack = 6m,
            PackSize = 20,
        };

        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), profile, Today);

        Assert.Equal(15m, snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_PerCigaretteTakesPrecedenceOverPack()
    {
        // قیمت هر نخ ۰٫۵ بر قیمت بسته برتری دارد.
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0.5m,
            PricePerPack = 6m,
            PackSize = 20,
        };

        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), profile, Today);

        Assert.Equal(25m, snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_ZeroPrice_IsNull()
    {
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerCigarette = 0m,
        };

        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), profile, Today);

        Assert.Null(snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_ZeroPackSize_IsNull()
    {
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 10,
            PricePerPack = 6m,
            PackSize = 0,
        };

        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), profile, Today);

        Assert.Null(snap.ApproximateSavings);
    }

    [Fact]
    public void Savings_FractionalPrice_ComputesPrecisely()
    {
        // ترک ۵ روز، ۷ نخ/روز، ۰٫۲۵ هر نخ → ۵×۷×۰٫۲۵ = ۸٫۷۵
        var profile = new UserProfile
        {
            QuitDateUtc = new DateTimeOffset(2024, 1, 5, 0, 0, 0, TimeSpan.Zero),
            CigarettesPerDay = 7,
            PricePerCigarette = 0.25m,
        };

        var snap = ProgressCalculator.Calculate(Array.Empty<LogEvent>(), profile, Today);

        Assert.Equal(8.75m, snap.ApproximateSavings);
    }
}
