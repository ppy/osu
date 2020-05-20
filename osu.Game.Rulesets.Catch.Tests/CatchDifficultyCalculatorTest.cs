// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class CatchDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Catch";

        // Due to precision issue in difficulty calculator,
        // the SR with MR on is not exactly the same with MR off.

        [TestCase(4.050601681491468d, "diffcalc-test")]
        public void Test(double expected, string name)
            => base.Test(expected, name);

        [TestCase(4.0506016929676321d, "diffcalc-test")]
        public void TestMirror(double expected, string name)
            => Test(expected, name, new CatchModMirror());

        [TestCase(4.4882724679198711d, "diffcalc-test")]
        public void TestHardRock(double expected, string name)
            => Test(expected, name, new CatchModHardRock());

        [TestCase(4.4882724606861499d, "diffcalc-test")]
        public void TestHardRockMirror(double expected, string name)
            => Test(expected, name, new CatchModHardRock(), new CatchModMirror());

        protected override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new CatchDifficultyCalculator(new CatchRuleset(), beatmap);

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }
}
