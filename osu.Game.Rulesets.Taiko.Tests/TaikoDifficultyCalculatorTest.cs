// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TaikoDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Taiko";

        [TestCase(1.9971301024093662d, 200, "diffcalc-test")]
        [TestCase(1.9971301024093662d, 200, "diffcalc-test-strong")]
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(3.1645810961313674d, 200, "diffcalc-test")]
        [TestCase(3.1645810961313674d, 200, "diffcalc-test-strong")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new TaikoModDoubleTime());

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new TaikoDifficultyCalculator(new TaikoRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}
