// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModFlashlight : OsuModTestScene
    {
        [Test]
        public void TestDisabledFollowPoints() => CreateModTest(new ModTestData
        {
            PassCondition = () => !((OsuPlayfield)Player.DrawableRuleset.Playfield).FollowPoints.IsPresent,
            Mod = new OsuModFlashlight
            {
                DisableFollowPoints = { Value = true }
            }
        });
    }
}
