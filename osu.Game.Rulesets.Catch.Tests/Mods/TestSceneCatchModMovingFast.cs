// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public partial class TestSceneCatchModMovingFast : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestMovingFast() => CreateModTest(new ModTestData
        {
            Mod = new CatchModMovingFast(),
            PassCondition = () => true
        });
    }
}
