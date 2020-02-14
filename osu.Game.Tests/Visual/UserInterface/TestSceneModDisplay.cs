// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModDisplay : OsuTestScene
    {
        [TestCase(ExpansionMode.ExpandOnHover)]
        [TestCase(ExpansionMode.AlwaysExpanded)]
        [TestCase(ExpansionMode.AlwaysContracted)]
        public void TestMode(ExpansionMode mode)
        {
            AddStep("create mod display", () =>
            {
                Child = new ModDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    ExpansionMode = mode,
                    Current =
                    {
                        Value = new Mod[]
                        {
                            new OsuModHardRock(),
                            new OsuModDoubleTime(),
                            new OsuModDifficultyAdjust(),
                            new OsuModEasy(),
                        }
                    }
                };
            });
        }
    }
}
