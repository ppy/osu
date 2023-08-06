// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModAccelerate : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestModAccelerate() => CreateModTest(new ModTestData
        {
            Mod = new ManiaModAccelerate
            {
                MaxComboCount = { Value = 2 },
                MaxScrollSpeed = { Value = 35 }
            },
            PassCondition = () =>
            {
                var drawableRuleset = (DrawableManiaRuleset)Player.DrawableRuleset;

                // CustomSmoothTimeRange cannot reach targetScrollTime because interpolates handle, so check this faster then speed 34.
                return !drawableRuleset.CustomSmoothTimeRange.Disabled && drawableRuleset.CustomSmoothTimeRange.Value <= DrawableManiaRuleset.ComputeScrollTime(34);
            }
        });
    }
}
