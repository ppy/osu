// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFooterButtonMods : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(FooterButtonMods)
        };

        private readonly TestFooterButtonMods footerButtonMods;

        public TestSceneFooterButtonMods()
        {
            Add(footerButtonMods = new TestFooterButtonMods());
        }

        [Test]
        public void TestIncrementMultiplier()
        {
            var hiddenMod = new Mod[] { new OsuModHidden() };
            AddStep(@"Add Hidden", () => changeMods(hiddenMod));
            AddAssert(@"Check Hidden multiplier", () => assertModsMultiplier(hiddenMod));

            var hardRockMod = new Mod[] { new OsuModHardRock() };
            AddStep(@"Add HardRock", () => changeMods(hardRockMod));
            AddAssert(@"Check HardRock multiplier", () => assertModsMultiplier(hardRockMod));

            var doubleTimeMod = new Mod[] { new OsuModDoubleTime() };
            AddStep(@"Add DoubleTime", () => changeMods(doubleTimeMod));
            AddAssert(@"Check DoubleTime multiplier", () => assertModsMultiplier(doubleTimeMod));

            var mutlipleIncrementMods = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModHardRock() };
            AddStep(@"Add multiple Mods", () => changeMods(mutlipleIncrementMods));
            AddAssert(@"Check multiple mod multiplier", () => assertModsMultiplier(mutlipleIncrementMods));
        }

        [Test]
        public void TestDecrementMultiplier()
        {
            var easyMod = new Mod[] { new OsuModEasy() };
            AddStep(@"Add Easy", () => changeMods(easyMod));
            AddAssert(@"Check Easy multiplier", () => assertModsMultiplier(easyMod));

            var noFailMod = new Mod[] { new OsuModNoFail() };
            AddStep(@"Add NoFail", () => changeMods(noFailMod));
            AddAssert(@"Check NoFail multiplier", () => assertModsMultiplier(noFailMod));

            var multipleDecrementMods = new Mod[] { new OsuModEasy(), new OsuModNoFail() };
            AddStep(@"Add Multiple Mods", () => changeMods(multipleDecrementMods));
            AddAssert(@"Check multiple mod multiplier", () => assertModsMultiplier(multipleDecrementMods));
        }

        [Test]
        public void TestClearMultiplier()
        {
            var multipleMods = new Mod[] { new OsuModDoubleTime(), new OsuModFlashlight() };
            AddStep(@"Add mods", () => changeMods(multipleMods));
            AddStep(@"Clear selected mod", () => changeMods(Array.Empty<Mod>()));
            AddAssert(@"Check empty multiplier", () => assertModsMultiplier(Array.Empty<Mod>()));
        }

        private void changeMods(IReadOnlyList<Mod> mods)
        {
            footerButtonMods.Current.Value = mods;
        }

        private bool assertModsMultiplier(IEnumerable<Mod> mods)
        {
            var multiplier = mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);
            var expectedValue = multiplier.Equals(1.0) ? string.Empty : $"{multiplier:N2}x";

            return expectedValue == footerButtonMods.MultiplierText.Text;
        }

        private class TestFooterButtonMods : FooterButtonMods
        {
            public new OsuSpriteText MultiplierText => base.MultiplierText;
        }
    }
}
