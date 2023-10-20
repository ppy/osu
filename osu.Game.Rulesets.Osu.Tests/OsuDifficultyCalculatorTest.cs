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
        protected override string ResourceAssembly => "osu.Game.Rulesets.Osu";

        [TestCase(6.7115569159190587d, 206, "diffcalc-test")]
        [TestCase(1.4391311903612753d, 45, "zero-length-sliders")]
        [TestCase(0.42506480230838789d, 2, "very-fast-slider")]
        [TestCase(0.14102693012101306d, 1, "nan-slider")]
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(8.9757300665532966d, 206, "diffcalc-test")]
        [TestCase(0.55071082800473514d, 2, "very-fast-slider")]
        [TestCase(1.7437232654020756d, 45, "zero-length-sliders")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModDoubleTime());

        [TestCase(6.7115569159190587d, 239, "diffcalc-test")]
        [TestCase(0.42506480230838789d, 4, "very-fast-slider")]
        [TestCase(1.4391311903612753d, 54, "zero-length-sliders")]
        public void TestClassicMod(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModClassic());

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
