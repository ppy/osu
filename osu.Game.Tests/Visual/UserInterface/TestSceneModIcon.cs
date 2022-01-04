// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Osu;
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

        [Test]
        public void TestInterfaceModType()
        {
            ModIcon icon = null;

            var ruleset = new OsuRuleset();

            AddStep("create mod icon", () => Child = icon = new ModIcon(ruleset.AllMods.First(m => m.Acronym == "DT")));
            AddStep("change mod", () => icon.Mod = ruleset.AllMods.First(m => m.Acronym == "EZ"));
        }
    }
}
