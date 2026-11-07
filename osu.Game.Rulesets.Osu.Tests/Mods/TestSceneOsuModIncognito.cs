// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.UI;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModIncognito : OsuModTestScene
    {
        [Test]
        public void TestDisableFollowPoints() => CreateModTest(new ModTestData
        {
            Mod = new OsuModIncognito { DisableFollowPoints = { Value = true } },
            PassCondition = () => !((DrawableOsuRuleset)Player.DrawableRuleset).Playfield.FollowPoints.IsPresent
        });

        [Test]
        public void TestNoComboColors()
        {
            CreateModTest(new ModTestData
            {
                Mod = new OsuModIncognito { NoComboColours = { Value = true } },
                PassCondition = () => true
            });

            AddStep("Skip to first", () => Player.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.HitObjects.First().StartTime));

            AddAssert("Combo colours are obscured", () => Player.DrawableRuleset.Playfield.HitObjectContainer.AliveObjects.All(o => o.AccentColour.Value == Color4.White));
        }
    }
}
