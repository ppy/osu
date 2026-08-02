// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Osu.Tests";

        [TestCase(6.5243170265483581d, 239, "diffcalc-test")]
        [TestCase(1.3280410795791415d, 54, "zero-length-sliders")]
        [TestCase(0.40867325147697559d, 4, "very-fast-slider")]
        [TestCase(0.87058175794353554d, 6, "nan-slider")]
        [TestCase(6.3059767387139756d, 2359, "801165")] // real world test
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(9.4677607900646308d, 239, "diffcalc-test")]
        [TestCase(1.6856612715618886d, 54, "zero-length-sliders")]
        [TestCase(0.53588473186572561d, 4, "very-fast-slider")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModDoubleTime());

        [TestCase(6.5243170265483581d, 239, "diffcalc-test")]
        [TestCase(1.3280410795791415d, 54, "zero-length-sliders")]
        [TestCase(0.40867325147697559d, 4, "very-fast-slider")]
        public void TestClassicMod(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModClassic());

        [Test]
        public void TestTimedExtended()
        {
            var beatmap = GetBeatmap("diffcalc-test");

            Dictionary<double, (double StarRating, int MaxCombo)> timedAttributes = CreateDifficultyCalculator(beatmap)
                                                                                    .CalculateTimed()
                                                                                    .ToDictionary(a => a.Time, a => (a.Attributes.StarRating, a.Attributes.MaxCombo));

            Assert.That(timedAttributes[500].MaxCombo, Is.EqualTo(1));
            Assert.That(timedAttributes[500].StarRating, Is.EqualTo(0).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[1000].MaxCombo, Is.EqualTo(2));
            Assert.That(timedAttributes[1000].StarRating, Is.EqualTo(0.16948857823852109).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[1500].MaxCombo, Is.EqualTo(3));
            Assert.That(timedAttributes[1500].StarRating, Is.EqualTo(0.26606949612932967).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[2000].MaxCombo, Is.EqualTo(4));
            Assert.That(timedAttributes[2000].StarRating, Is.EqualTo(0.35775631754708426).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[2500].MaxCombo, Is.EqualTo(5));
            Assert.That(timedAttributes[2500].StarRating, Is.EqualTo(0.56319343599556226).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[17000].MaxCombo, Is.EqualTo(62));
            Assert.That(timedAttributes[17000].StarRating, Is.EqualTo(5.6685465406782036).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[18000].MaxCombo, Is.EqualTo(63));
            Assert.That(timedAttributes[18000].StarRating, Is.EqualTo(5.7272669362777702).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[18125].MaxCombo, Is.EqualTo(64));
            Assert.That(timedAttributes[18125].StarRating, Is.EqualTo(5.7347762555961532).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[61500].MaxCombo, Is.EqualTo(160));
            Assert.That(timedAttributes[61500].StarRating, Is.EqualTo(6.5240565852553738).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[70000].MaxCombo, Is.EqualTo(176));
            Assert.That(timedAttributes[70000].StarRating, Is.EqualTo(6.5240609777925842).Within(CHECK_PRECISION));
            Assert.That(timedAttributes[74600].MaxCombo, Is.EqualTo(195));
            Assert.That(timedAttributes[74600].StarRating, Is.EqualTo(6.5240617806983261).Within(CHECK_PRECISION));
        }

        [TestCase(239, "diffcalc-test")]
        [TestCase(54, "zero-length-sliders")]
        [TestCase(4, "very-fast-slider")]
        public void TestOffsetChanges(int expectedMaxCombo, string name)
        {
            const double offset_iterations = 400;
            var beatmap = GetBeatmap(name);

            var expectedAttributes = CreateDifficultyCalculator(beatmap).Calculate();

            for (int i = 0; i < offset_iterations; i++)
            {
                foreach (var beatmapHitObject in beatmap.Beatmap.HitObjects)
                    beatmapHitObject.StartTime++;

                var calc = CreateDifficultyCalculator(beatmap);

                var attributes = calc.Calculate();
                var timedAttributes = calc.CalculateTimed();

                Assert.That(attributes, Is.EqualTo(expectedAttributes).UsingPropertiesComparer());
                Assert.That(timedAttributes.Last().Attributes, Is.EqualTo(expectedAttributes).UsingPropertiesComparer());
            }
        }

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
