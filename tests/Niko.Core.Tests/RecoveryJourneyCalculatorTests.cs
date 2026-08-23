// ============================================================================
// Niko.Core.Tests — RecoveryJourneyCalculatorTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: اثبات نگاشت قطعی و غیرپزشکی مراحل Recovery به Island.
// وابستگی‌ها و لایه: لایهٔ تست → Domain/Recovery در Core؛ بدون I/O یا شبکه.
// نکات تغییر و قیود: این تست‌ها تضمین می‌کنند UI از آستانهٔ ساختگی یا دادهٔ خصوصی
//           برای رشد Island استفاده نکند.
// ============================================================================

using Niko.Core.Domain.Recovery;

namespace Niko.Core.Tests;

public sealed class RecoveryJourneyCalculatorTests
{
    [Theory]
    [InlineData(RecoveryStage.Stage0, RecoveryJourneyStage.Seedling)]
    [InlineData(RecoveryStage.Stage1, RecoveryJourneyStage.Seedling)]
    [InlineData(RecoveryStage.Stage2, RecoveryJourneyStage.Garden)]
    [InlineData(RecoveryStage.Stage3, RecoveryJourneyStage.Garden)]
    [InlineData(RecoveryStage.Stage4, RecoveryJourneyStage.Forest)]
    [InlineData(RecoveryStage.Stage5, RecoveryJourneyStage.Forest)]
    [InlineData(RecoveryStage.Stage6, RecoveryJourneyStage.Haven)]
    [InlineData(RecoveryStage.Stage7, RecoveryJourneyStage.Haven)]
    public void Calculate_WithRecoveryData_MapsEachRecoveryRangeToOneVisualStage(
        RecoveryStage recoveryStage,
        RecoveryJourneyStage expected)
    {
        var result = RecoveryJourneyCalculator.Calculate(new RecoverySnapshot
        {
            Stage = recoveryStage,
            HasSufficientData = true,
        });

        Assert.True(result.IsAvailable);
        Assert.Equal(expected, result.Stage);
    }

    [Fact]
    public void Calculate_WithoutSufficientData_ReturnsSafeUnavailableSeedling()
    {
        var result = RecoveryJourneyCalculator.Calculate(new RecoverySnapshot
        {
            Stage = RecoveryStage.Stage7,
            HasSufficientData = false,
        });

        Assert.False(result.IsAvailable);
        Assert.Equal(RecoveryJourneyStage.Seedling, result.Stage);
    }
}
