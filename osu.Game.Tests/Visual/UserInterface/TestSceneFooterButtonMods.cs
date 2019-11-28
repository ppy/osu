// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
            AddStep(@"Add Hidden", () => changeMods(new Mod[] { new OsuModHidden() }));
            AddAssert(@"Check Hidden multiplier", () => footerButtonMods.MultiplierText.Text == @"1.06x");
            AddStep(@"Add HardRock", () => changeMods(new Mod[] { new OsuModHidden() }));
            AddAssert(@"Check HardRock multiplier", () => footerButtonMods.MultiplierText.Text == @"1.06x");
            AddStep(@"Add DoubleTime", () => changeMods(new Mod[] { new OsuModDoubleTime() }));
            AddAssert(@"Check DoubleTime multiplier", () => footerButtonMods.MultiplierText.Text == @"1.12x");
            AddStep(@"Add multiple Mods", () => changeMods(new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModHidden() }));
            AddAssert(@"Check multiple mod multiplier", () => footerButtonMods.MultiplierText.Text == @"1.26x");
        }

        [Test]
        public void TestDecrementMultiplier()
        {
            AddStep(@"Add Easy", () => changeMods(new Mod[] { new OsuModEasy() }));
            AddAssert(@"Check Easy multiplier", () => footerButtonMods.MultiplierText.Text == @"0.50x");
            AddStep(@"Add NoFail", () => changeMods(new Mod[] { new OsuModNoFail() }));
            AddAssert(@"Check NoFail multiplier", () => footerButtonMods.MultiplierText.Text == @"0.50x");
            AddStep(@"Add Multiple Mods", () => changeMods(new Mod[] { new OsuModEasy(), new OsuModNoFail() }));
            AddAssert(@"Check multiple mod multiplier", () => footerButtonMods.MultiplierText.Text == @"0.25x");
        }

        [Test]
        public void TestClearMultiplier()
        {
            AddStep(@"Add mods", () => changeMods(new Mod[] { new OsuModDoubleTime(), new OsuModFlashlight() }));
            AddStep(@"Clear selected mod", () => changeMods(Array.Empty<Mod>()));
            AddAssert(@"Check empty multiplier", () => string.IsNullOrEmpty(footerButtonMods.MultiplierText.Text));
        }

        private void changeMods(IReadOnlyList<Mod> mods)
        {
            footerButtonMods.Current.Value = mods;
        }

        private class TestFooterButtonMods : FooterButtonMods
        {
            public new OsuSpriteText MultiplierText => base.MultiplierText;
        }
    }
}
