using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace BetterPawnControlForked.Tests
{
    public class LocalizationTests
    {
        [Theory]
        [InlineData("ChineseSimplified/Keyed/BetterPawnControlForked.xml")]
        [InlineData("Japanese/Keyed/BetterPawnControlForked.xml")]
        [InlineData("Japanese/DefInjected/KeyBindingDef/KeyBindings.xml")]
        [InlineData("Japanese/DefInjected/KeyBindingCategoryDef/KeyBindingCategories.xml")]
        public void TranslationsAreValidUtf8AndRetainLocalizedText(string relativePath)
        {
            var document = LoadTranslation(relativePath);
            Assert.Equal("LanguageData", document.Root.Name.LocalName);
            var entries = document.Root.Elements().ToArray();
            Assert.NotEmpty(entries);
            Assert.Equal(entries.Length, entries.Select(entry => entry.Name).Distinct().Count());

            foreach (var entry in entries)
            {
                Assert.False(string.IsNullOrWhiteSpace(entry.Value), $"{relativePath}: {entry.Name} is empty.");
                Assert.DoesNotContain("\uFFFD", entry.Value);
                Assert.False(Regex.IsMatch(entry.Value, @"\?{2,}"), $"{relativePath}: {entry.Name} contains question-mark corruption.");

                // Product names intentionally remain in English; other entries contain translated text.
                if (entry.Name.LocalName == "BPC.BetterPawnControlForked" ||
                    entry.Name.LocalName == "BetterPawnControlForked.label" ||
                    (relativePath.StartsWith("Japanese/", StringComparison.Ordinal) &&
                     entry.Name.LocalName == "BPC.BetterPawnControlForkedSettingsName"))
                {
                    Assert.Equal("Better Pawn Control Forked", entry.Value);
                    continue;
                }

                Assert.True(Regex.IsMatch(entry.Value, @"[\u3040-\u30FF\u3400-\u9FFF]"),
                    $"{relativePath}: {entry.Name} has lost its localized text.");
            }
        }

        [Theory]
        [InlineData("ChineseSimplified", "默认", "重新命名")]
        [InlineData("Japanese", "自動", "名前の変更")]
        public void ReportedLabelsMatchOriginalTranslations(string language, string auto, string rename)
        {
            var document = LoadTranslation(language + "/Keyed/BetterPawnControlForked.xml");
            Assert.Equal(auto, document.Root.Element("BPC.Auto")?.Value);
            Assert.Equal(rename, document.Root.Element("BPC.Rename")?.Value);
        }

        private static XDocument LoadTranslation(string relativePath)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Languages", relativePath);
            var text = new UTF8Encoding(false, true).GetString(File.ReadAllBytes(path));
            return XDocument.Parse(text.TrimStart('\uFEFF'));
        }
    }
}
