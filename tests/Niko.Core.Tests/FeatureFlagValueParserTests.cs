// نام فایل: FeatureFlagValueParserTests.cs
// مسئولیت: آزمون تفسیر deterministic و fail-closed مقدار feature flag.
// وابستگی‌ها و لایه: تست Core → قرارداد FeatureFlagValueParser؛ بدون UI، شبکه یا داده واقعی.
// نکات تغییر و قیود: پیش‌فرض فعال و مقادیر نامعتبر خاموش باید پایدار بمانند.

using Niko.Core.Abstractions;

namespace Niko.Core.Tests;

public sealed class FeatureFlagValueParserTests
{
    [Fact]
    public void Parse_EmptyValue_UsesDefault()
    {
        Assert.True(FeatureFlagValueParser.Parse(null, defaultValue: true));
        Assert.False(FeatureFlagValueParser.Parse("", defaultValue: false));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("on")]
    [InlineData("enabled")]
    public void Parse_EnabledValues_ReturnsTrue(string value)
        => Assert.True(FeatureFlagValueParser.Parse(value, defaultValue: false));

    [Theory]
    [InlineData("0")]
    [InlineData("false")]
    [InlineData("off")]
    [InlineData("disabled")]
    [InlineData("unexpected")]
    public void Parse_DisabledOrUnknownValues_ReturnsFalse(string value)
        => Assert.False(FeatureFlagValueParser.Parse(value, defaultValue: true));
}
