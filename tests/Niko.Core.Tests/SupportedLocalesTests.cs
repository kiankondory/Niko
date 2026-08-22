// ============================================================================
// Niko.Core.Tests — SupportedLocalesTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: آزمون فهرست localeهای P1.8، جهت RTL و علامت‌گذاری fallback ترجمه.
// وابستگی‌ها و لایه: لایهٔ تست؛ فقط Domain/Localization در Core را مصرف می‌کند.
// نکات تغییر و قیود: فهرست باید قطعی، کامل و مستقل از سیستم‌عامل باشد.
// ============================================================================

using Niko.Core.Domain.Localization;

namespace Niko.Core.Tests;

public sealed class SupportedLocalesTests
{
    [Fact]
    public void All_ContainsConfiguredLocaleList()
    {
        var expected = new[]
        {
            "en", "fa", "ar", "de", "es", "fr", "hi", "id",
            "ja", "ko", "pt-BR", "ru", "tr", "uk", "zh-Hans", "zh-Hant",
        };

        Assert.Equal(expected, SupportedLocales.All.Select(locale => locale.Code));
    }

    [Fact]
    public void OnlyFullyTranslatedLocalesAreMarkedComplete()
    {
        var complete = SupportedLocales.All
            .Where(locale => locale.IsFullyTranslated)
            .Select(locale => locale.Code);

        Assert.Equal(new[] { "en", "fa", "ar", "zh-Hans" }, complete);
    }

    [Fact]
    public void PersianAndArabicAreRtl()
    {
        Assert.True(SupportedLocales.Get("fa").IsRightToLeft);
        Assert.True(SupportedLocales.Get("ar").IsRightToLeft);
        Assert.False(SupportedLocales.Get("en").IsRightToLeft);
    }
}
