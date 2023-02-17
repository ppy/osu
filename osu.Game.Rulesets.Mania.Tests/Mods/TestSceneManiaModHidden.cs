// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModHidden : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [TestCase(0.3f, 0.6f)]
        [TestCase(0.2f, 0.2f)]
        [TestCase(0.8f, 0.8f)]
        [TestCase(0.2f, 0.8f)]
        public void TestCoverage(float minCoverage, float maxCoverage) => CreateModTest(new ModTestData { Mod = new ManiaModHidden { MinCoverage = { Value = minCoverage }, MaxCoverage = { Value = maxCoverage } }, PassCondition = () => true });
    }
}
