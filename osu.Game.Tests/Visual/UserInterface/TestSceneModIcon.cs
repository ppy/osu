// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneModIcon : OsuTestScene
    {
        [Test]
        public void TestChangeModType()
        {
            ModIcon icon = null;

            AddStep("create mod icon", () => Child = icon = new ModIcon(new OsuModDoubleTime()));
            AddStep("change mod", () => icon.Mod = new OsuModEasy());
        }
    }
}
