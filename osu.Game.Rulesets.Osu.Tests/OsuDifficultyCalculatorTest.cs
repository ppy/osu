﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        [TestCase(6.7171144000821119d, 239, "diffcalc-test")]
        [TestCase(1.4485749025771304d, 54, "zero-length-sliders")]
        [TestCase(0.42630400627180914d, 4, "very-fast-slider")]
        [TestCase(0.14143808967817237d, 2, "nan-slider")]
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(8.9825709931204205d, 239, "diffcalc-test")]
        [TestCase(1.7550169162648608d, 54, "zero-length-sliders")]
        [TestCase(0.55231632896800109d, 4, "very-fast-slider")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModDoubleTime());

        [TestCase(6.7171144000821119d, 239, "diffcalc-test")]
        [TestCase(1.4485749025771304d, 54, "zero-length-sliders")]
        [TestCase(0.42630400627180914d, 4, "very-fast-slider")]
        public void TestClassicMod(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new OsuModClassic());

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new OsuDifficultyCalculator(new OsuRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
