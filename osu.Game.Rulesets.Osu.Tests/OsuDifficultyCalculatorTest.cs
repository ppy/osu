// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        [TestCase(6.6232533278125061d, 239, "diffcalc-test")]
        [TestCase(1.5045783545699611d, 54, "zero-length-sliders")]
        [TestCase(0.43333836671191595d, 4, "very-fast-slider")]
        [TestCase(0.13841532030395723d, 2, "nan-slider")]
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(9.6491691624112761d, 239, "diffcalc-test")]
        [TestCase(1.756936832498702d, 54, "zero-length-sliders")]
        [TestCase(0.57771197086735004d, 4, "very-fast-slider")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModDoubleTime());

        [TestCase(6.6232533278125061d, 239, "diffcalc-test")]
        [TestCase(1.5045783545699611d, 54, "zero-length-sliders")]
        [TestCase(0.43333836671191595d, 4, "very-fast-slider")]
        public void TestClassicMod(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModClassic());

        [TestCase(239, "diffcalc-test")]
        [TestCase(54, "zero-length-sliders")]
        [TestCase(4, "very-fast-slider")]
        public void TestOffsetChanges(int expectedMaxCombo, string name)
        {
            const double offset_iterations = 400;
            var beatmap = GetBeatmap(name);

            var attributes = CreateDifficultyCalculator(beatmap).Calculate();
            double expectedStarRating = attributes.StarRating;

            for (int i = 0; i < offset_iterations; i++)
            {
                foreach (var beatmapHitObject in beatmap.Beatmap.HitObjects)
                    beatmapHitObject.StartTime++;

                attributes = CreateDifficultyCalculator(beatmap).Calculate();

                // Platform-dependent math functions (Pow, Cbrt, Exp, etc) may result in minute differences.
                Assert.That(attributes.StarRating, Is.EqualTo(expectedStarRating).Within(0.00001));
                Assert.That(attributes.MaxCombo, Is.EqualTo(expectedMaxCombo));
            }
        }

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
