// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public partial class TestSceneCatchModCover : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [TestCase(0.2f)]
        [TestCase(0.5f)]
        [TestCase(0.8f)]
        public void TestCoverTop(float coverage) => CreateModTest(new ModTestData
        {
            Mod = new CatchModCover
            {
                Direction = { Value = CoverExpandDirection.AlongScroll },
                Coverage = { Value = coverage }
            },
            PassCondition = () => true
        });

        [TestCase(0.2f)]
        [TestCase(0.5f)]
        [TestCase(0.8f)]
        public void TestCoverBottom(float coverage) => CreateModTest(new ModTestData
        {
            Mod = new CatchModCover
            {
                Direction = { Value = CoverExpandDirection.AgainstScroll },
                Coverage = { Value = coverage }
            },
            PassCondition = () => true
        });
    }
}