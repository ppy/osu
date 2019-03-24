// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class CatchDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Catch";

        [TestCase(4.2058561036909863d, "diffcalc-test")]
        public void Test(double expected, string name)
            => base.Test(expected, name);

        protected override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new CatchDifficultyCalculator(new CatchRuleset(), beatmap);

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }
}
