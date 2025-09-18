// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    public class ManiaDifficultyCalculatorTest : DifficultyCalculatorTest
    {
        protected override string ResourceAssembly => "osu.Game.Rulesets.Mania.Tests";

        [TestCase(2.3493769750220914d, 242, "diffcalc-test")]
        public void Test(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(8.5654363351334357, 4200, "onus")]
        public void Test2(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);


        [TestCase(11.38, 4200, "everythingblack")]
        public void TestEBDT(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name, new ManiaModDoubleTime());


        [TestCase(7.0093187284434917, 4200, "purple")]
        public void Test3(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(6.8369, 4200, "revenge")]
        public void Test4(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);
       [TestCase(10.3071, 4200, "eta")]
            public void Test5(double expectedStarRating, int expectedMaxCombo, string name)
                => base.Test(expectedStarRating, expectedMaxCombo, name);
       [TestCase(8.1490, 4200, "delta")]
            public void Test6(double expectedStarRating, int expectedMaxCombo, string name)
                => base.Test(expectedStarRating, expectedMaxCombo, name);
       [TestCase(9.1171, 4200, "epsilon")]
            public void Test7(double expectedStarRating, int expectedMaxCombo, string name)
                => base.Test(expectedStarRating, expectedMaxCombo, name);
       [TestCase(7.5991, 4200, "everythingblack")]
            public void Test8(double expectedStarRating, int expectedMaxCombo, string name)
                => base.Test(expectedStarRating, expectedMaxCombo, name);
       [TestCase(6.6925, 4200, "aiae")]
            public void Test9(double expectedStarRating, int expectedMaxCombo, string name)
                => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(7.0456659214552246, 4200, "theempress")]
        public void Test10(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);


        [TestCase(6.4445755674348142, 6165, "weeny")]
        public void Test11(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(8.82, 6165, "azure")]
        public void TestAzure(double expectedStarRating, int expectedMaxCombo, string name)
            => base.Test(expectedStarRating, expectedMaxCombo, name);

        [TestCase(8.5859968966934961, 6165, "weeny")]
        public void TestWeenyDT(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new ManiaModDoubleTime());


        /*[TestCase(2.797245912537965d, 242, "diffcalc-test")]
        public void TestClockRateAdjusted(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new ManiaModDoubleTime());*/

        [TestCase(10.003616236734443, 4681, "aiae")]
        public void TestClockRateAdjustedAiae(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new ManiaModDoubleTime());

        [TestCase(6.5345158134107413, 3994, "triumph")]
        public void TestClockRateAdjustedTriumph(double expectedStarRating, int expectedMaxCombo, string name)
            => Test(expectedStarRating, expectedMaxCombo, name, new ManiaModDoubleTime());

        protected override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => new ManiaDifficultyCalculator(new ManiaRuleset().RulesetInfo, beatmap);

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }
}
