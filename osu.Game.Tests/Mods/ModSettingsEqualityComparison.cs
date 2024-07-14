// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.API;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class ModSettingsEqualityComparison
    {
        [Test]
        public void TestAPIMod()
        {
            var apiMod1 = new APIMod(new OsuModDoubleTime { SpeedChange = { Value = 1.25 } });
            var apiMod2 = new APIMod(new OsuModDoubleTime { SpeedChange = { Value = 1.26 } });
            var apiMod3 = new APIMod(new OsuModDoubleTime { SpeedChange = { Value = 1.26 } });

            Assert.That(apiMod1, Is.Not.EqualTo(apiMod2));
            Assert.That(apiMod2, Is.EqualTo(apiMod2));
            Assert.That(apiMod2, Is.EqualTo(apiMod3));
            Assert.That(apiMod3, Is.EqualTo(apiMod2));
        }

        [Test]
        public void TestMod()
        {
            var ruleset = new OsuRuleset();

            var mod1 = new OsuModDoubleTime { SpeedChange = { Value = 1.25 } };
            var mod2 = new OsuModDoubleTime { SpeedChange = { Value = 1.26 } };
            var mod3 = new OsuModDoubleTime { SpeedChange = { Value = 1.26 } };

            var doubleConvertedMod1 = new APIMod(mod1).ToMod(ruleset);
            var doubleConvertedMod2 = new APIMod(mod2).ToMod(ruleset);
            var doubleConvertedMod3 = new APIMod(mod3).ToMod(ruleset);

            Assert.That(mod1, Is.Not.EqualTo(mod2));
            Assert.That(doubleConvertedMod1, Is.Not.EqualTo(doubleConvertedMod2));

            Assert.That(mod2, Is.EqualTo(mod2));
            Assert.That(doubleConvertedMod2, Is.EqualTo(doubleConvertedMod2));

            Assert.That(mod2, Is.EqualTo(mod3));
            Assert.That(doubleConvertedMod2, Is.EqualTo(doubleConvertedMod3));

            Assert.That(mod3, Is.EqualTo(mod2));
            Assert.That(doubleConvertedMod3, Is.EqualTo(doubleConvertedMod2));
        }

        [Test]
        public void TestModWithMultipleSettings()
        {
            var ruleset = new OsuRuleset();

            var mod1 = new OsuModDifficultyAdjust { OverallDifficulty = { Value = 10 }, CircleSize = { Value = 0 } };
            var mod2 = new OsuModDifficultyAdjust { OverallDifficulty = { Value = 10 }, CircleSize = { Value = 6 } };
            var mod3 = new OsuModDifficultyAdjust { OverallDifficulty = { Value = 10 }, CircleSize = { Value = 6 } };

            var doubleConvertedMod1 = new APIMod(mod1).ToMod(ruleset);
            var doubleConvertedMod2 = new APIMod(mod2).ToMod(ruleset);
            var doubleConvertedMod3 = new APIMod(mod3).ToMod(ruleset);

            Assert.That(mod1, Is.Not.EqualTo(mod2));
            Assert.That(doubleConvertedMod1, Is.Not.EqualTo(doubleConvertedMod2));

            Assert.That(mod2, Is.EqualTo(mod2));
            Assert.That(doubleConvertedMod2, Is.EqualTo(doubleConvertedMod2));

            Assert.That(mod2, Is.EqualTo(mod3));
            Assert.That(doubleConvertedMod2, Is.EqualTo(doubleConvertedMod3));

            Assert.That(mod3, Is.EqualTo(mod2));
            Assert.That(doubleConvertedMod3, Is.EqualTo(doubleConvertedMod2));
        }
    }
}
