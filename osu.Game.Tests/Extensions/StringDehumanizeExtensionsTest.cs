// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Globalization;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Extensions;

namespace osu.Game.Tests.Extensions
{
    [TestFixture]
    public class StringDehumanizeExtensionsTest
    {
        [Test]
        [TestCase("single", "Single")]
        [TestCase("example word", "ExampleWord")]
        [TestCase("mixed Casing test", "MixedCasingTest")]
        [TestCase("PascalCase", "PascalCase")]
        [TestCase("camelCase", "CamelCase")]
        [TestCase("snake_case", "SnakeCase")]
        [TestCase("kebab-case", "KebabCase")]
        [TestCase("i will not break in a different culture", "IWillNotBreakInADifferentCulture", "tr-TR")]
        public void TestToPascalCase(string input, string expectedOutput, string? culture = null)
        {
            using (temporaryCurrentCulture(culture))
                Assert.That(input.ToPascalCase(), Is.EqualTo(expectedOutput));
        }

        [Test]
        [TestCase("single", "single")]
        [TestCase("example word", "exampleWord")]
        [TestCase("mixed Casing test", "mixedCasingTest")]
        [TestCase("PascalCase", "pascalCase")]
        [TestCase("camelCase", "camelCase")]
        [TestCase("snake_case", "snakeCase")]
        [TestCase("kebab-case", "kebabCase")]
        [TestCase("I will not break in a different culture", "iWillNotBreakInADifferentCulture", "tr-TR")]
        public void TestToCamelCase(string input, string expectedOutput, string? culture = null)
        {
            using (temporaryCurrentCulture(culture))
                Assert.That(input.ToCamelCase(), Is.EqualTo(expectedOutput));
        }

        [Test]
        [TestCase("single", "single")]
        [TestCase("example word", "example_word")]
        [TestCase("mixed Casing test", "mixed_casing_test")]
        [TestCase("PascalCase", "pascal_case")]
        [TestCase("camelCase", "camel_case")]
        [TestCase("snake_case", "snake_case")]
        [TestCase("kebab-case", "kebab_case")]
        [TestCase("I will not break in a different culture", "i_will_not_break_in_a_different_culture", "tr-TR")]
        public void TestToSnakeCase(string input, string expectedOutput, string? culture = null)
        {
            using (temporaryCurrentCulture(culture))
                Assert.That(input.ToSnakeCase(), Is.EqualTo(expectedOutput));
        }

        [Test]
        [TestCase("single", "single")]
        [TestCase("example word", "example-word")]
        [TestCase("mixed Casing test", "mixed-casing-test")]
        [TestCase("PascalCase", "pascal-case")]
        [TestCase("camelCase", "camel-case")]
        [TestCase("snake_case", "snake-case")]
        [TestCase("kebab-case", "kebab-case")]
        [TestCase("I will not break in a different culture", "i-will-not-break-in-a-different-culture", "tr-TR")]
        public void TestToKebabCase(string input, string expectedOutput, string? culture = null)
        {
            using (temporaryCurrentCulture(culture))
                Assert.That(input.ToKebabCase(), Is.EqualTo(expectedOutput));
        }

        private IDisposable temporaryCurrentCulture(string? cultureName)
        {
            var storedCulture = CultureInfo.CurrentCulture;

            if (cultureName != null)
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);

            return new InvokeOnDisposal(() => CultureInfo.CurrentCulture = storedCulture);
        }
    }
}
