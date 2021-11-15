// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        [TestCase(2.2420075288523802d, "diffcalc-test")]
        [TestCase(2.2420075288523802d, "diffcalc-test-strong")]
        public void Test(double expected, string name)
            => base.Test(expected, name);

        [TestCase(3.134084469440479d, "diffcalc-test")]
        [TestCase(3.134084469440479d, "diffcalc-test-strong")]
        public void TestClockRateAdjusted(double expected, string name)
            => Test(expected, name, new TaikoModDoubleTime());

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new TaikoDifficultyCalculator(new TaikoRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}
