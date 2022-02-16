// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFooterButtonMods : OsuTestScene
    {
        private readonly FooterButtonMods footerButtonMods;

        public TestSceneFooterButtonMods()
        {
            Add(footerButtonMods = new FooterButtonMods());
        }

        [Test]
        public void TestIncrementMultiplier()
        {
            var hiddenMod = new Mod[] { new OsuModHidden() };
            AddStep(@"Add Hidden", () => changeMods(hiddenMod));

            var hardRockMod = new Mod[] { new OsuModHardRock() };
            AddStep(@"Add HardRock", () => changeMods(hardRockMod));

            var doubleTimeMod = new Mod[] { new OsuModDoubleTime() };
            AddStep(@"Add DoubleTime", () => changeMods(doubleTimeMod));

            var multipleIncrementMods = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModHardRock() };
            AddStep(@"Add multiple Mods", () => changeMods(multipleIncrementMods));
        }

        [Test]
        public void TestDecrementMultiplier()
        {
            var easyMod = new Mod[] { new OsuModEasy() };
            AddStep(@"Add Easy", () => changeMods(easyMod));

            var noFailMod = new Mod[] { new OsuModNoFail() };
            AddStep(@"Add NoFail", () => changeMods(noFailMod));

            var multipleDecrementMods = new Mod[] { new OsuModEasy(), new OsuModNoFail() };
            AddStep(@"Add Multiple Mods", () => changeMods(multipleDecrementMods));
        }

        [Test]
        public void TestClearMultiplier()
        {
            var multipleMods = new Mod[] { new OsuModDoubleTime(), new OsuModFlashlight() };
            AddStep(@"Add mods", () => changeMods(multipleMods));
            AddStep(@"Clear selected mod", () => changeMods(Array.Empty<Mod>()));
        }

        private void changeMods(IReadOnlyList<Mod> mods)
        {
            footerButtonMods.Current.Value = mods;
        }
    }
}
