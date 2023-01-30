// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModFadeIn : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [TestCase(0.5f)]
        [TestCase(0.1f)]
        [TestCase(0.7f)]
        public void TestCoverage(float coverage) => CreateModTest(new ModTestData { Mod = new ManiaModFadeIn { Coverage = { Value = coverage } }, PassCondition = () => true });
    }
}
