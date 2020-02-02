// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class ManiaDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Mania";

        [TestCase(2.3683365342338796d, "diffcalc-test")]
        public void Test(double expected, string name)
            => base.Test(expected, name);

        protected override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => new ManiaDifficultyCalculator(new ManiaRuleset(), beatmap);

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }
}
