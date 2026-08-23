// ============================================================================
// Niko.Infrastructure.Tests — LocalizationResourceCoverageTests.cs
// ----------------------------------------------------------------------------
// مسئولیت: اعتبارسنجی منابع locale برنامه و سازگاری placeholderهای آن‌ها.
// وابستگی‌ها و لایه: تست یکپارچه → resxهای MAUI و فهرست SupportedLocales در Core.
// نکات تغییر و قیود: ترجمهٔ جزئی مجاز است، اما کلید ناشناخته یا placeholder ناسازگار مجاز نیست.
// ============================================================================

using System.Text.RegularExpressions;
using System.Xml.Linq;
using Niko.Core.Domain.Localization;

namespace Niko.Infrastructure.Tests;

public sealed class LocalizationResourceCoverageTests
{
    private static readonly Regex Placeholder = new("\\{(?<index>\\d+)(?:[^}]*)\\}", RegexOptions.Compiled);
    private static readonly Regex HardCodedText = new("\\bText\\s*=\\s*\"(?!\\{|$)[^\"]+\"", RegexOptions.Compiled);

    [Fact]
    public void EveryConfiguredLocale_HasAResourceFile_AndOnlyUsesKnownKeys()
    {
        var resources = LoadResources();
        var neutral = resources["en"];

        foreach (var locale in SupportedLocales.All)
        {
            Assert.True(resources.TryGetValue(locale.Code, out var translations), $"Missing resource for {locale.Code}.");
            Assert.All(translations.Keys, key => Assert.Contains(key, neutral.Keys));
            foreach (var (key, translation) in translations)
            {
                Assert.Equal(PlaceholderSignature(neutral[key]), PlaceholderSignature(translation));
            }
        }
    }

    [Fact]
    public void FullyTranslatedLocales_HaveEveryNeutralKey()
    {
        var resources = LoadResources();
        var neutralKeys = resources["en"].Keys;

        foreach (var locale in SupportedLocales.All.Where(locale => locale.IsFullyTranslated))
        {
            Assert.Equal(neutralKeys.OrderBy(key => key), resources[locale.Code].Keys.OrderBy(key => key));
        }
    }

    [Fact]
    public void XamlPages_DoNotContainHardCodedVisibleText()
    {
        var applicationDirectory = Path.Combine(FindRepositoryRoot(), "Niko");
        var violations = Directory.GetFiles(applicationDirectory, "*.xaml", SearchOption.AllDirectories)
            .SelectMany(path => HardCodedText.Matches(File.ReadAllText(path))
                .Select(match => $"{Path.GetFileName(path)}: {match.Value}"))
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static Dictionary<string, Dictionary<string, string>> LoadResources()
    {
        var directory = Path.Combine(FindRepositoryRoot(), "Niko", "Resources", "Localization");
        return SupportedLocales.All.ToDictionary(
            locale => locale.Code,
            locale => Read(Path.Combine(directory, FileName(locale.Code))),
            StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, string> Read(string path)
        => XDocument.Load(path).Root!
            .Elements("data")
            .ToDictionary(
                element => element.Attribute("name")!.Value,
                element => element.Element("value")?.Value ?? string.Empty,
                StringComparer.Ordinal);

    private static string FileName(string locale)
        => locale == "en" ? "Localization.resx" : $"Localization.{locale}.resx";

    private static string PlaceholderSignature(string value)
        => string.Join(",", Placeholder.Matches(value).Select(match => match.Groups["index"].Value).OrderBy(index => index));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src", "Niko.Core")) &&
                Directory.Exists(Path.Combine(directory.FullName, "Niko", "Resources", "Localization")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Niko repository root was not found for localization validation.");
    }
}
