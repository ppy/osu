// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.SelectV2.Footer;
using osu.Game.Utils;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFooterButtonModsV2 : OsuTestScene
    {
        private readonly TestFooterButtonModsV2 footerButtonMods;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public TestSceneFooterButtonModsV2()
        {
            Add(footerButtonMods = new TestFooterButtonModsV2
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.CentreLeft,
                X = -100,
                Action = () => { },
            });
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("one mod", () => changeMods(new List<Mod> { new OsuModHidden() }));
            AddStep("two mods", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock() }));
            AddStep("three mods", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime() }));
            AddStep("four mods", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic() }));
            AddStep("five mods", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() }));

            AddStep("modified", () => changeMods(new List<Mod> { new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } }));
            AddStep("modified + one", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } }));
            AddStep("modified + two", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } } }));

            AddStep("clear mods", () => changeMods(Array.Empty<Mod>()));
            AddWaitStep("wait", 3);
            AddStep("one mod", () => changeMods(new List<Mod> { new OsuModHidden() }));

            AddStep("clear mods", () => changeMods(Array.Empty<Mod>()));
            AddWaitStep("wait", 3);
            AddStep("five mods", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime(), new OsuModClassic(), new OsuModDifficultyAdjust() }));
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

            var multipleIncrementMods = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModHardRock() };
            AddStep(@"Add multiple Mods", () => changeMods(multipleIncrementMods));
            AddAssert(@"Check multiple mod multiplier", () => assertModsMultiplier(multipleIncrementMods));
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
        public void TestUnrankedBadge()
        {
            AddStep(@"Add unranked mod", () => changeMods(new[] { new OsuModDeflate() }));
            AddUntilStep("Unranked badge shown", () => footerButtonMods.ChildrenOfType<FooterButtonModsV2.UnrankedBadge>().Single().Alpha == 1);
            AddStep(@"Clear selected mod", () => changeMods(Array.Empty<Mod>()));
            AddUntilStep("Unranked badge not shown", () => footerButtonMods.ChildrenOfType<FooterButtonModsV2.UnrankedBadge>().Single().Alpha == 0);
        }

        private void changeMods(IReadOnlyList<Mod> mods) => footerButtonMods.Current.Value = mods;

        private bool assertModsMultiplier(IEnumerable<Mod> mods)
        {
            double multiplier = mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);
            string expectedValue = ModUtils.FormatScoreMultiplier(multiplier).ToString();

            return expectedValue == footerButtonMods.MultiplierText.Current.Value;
        }

        private partial class TestFooterButtonModsV2 : FooterButtonModsV2
        {
            public new OsuSpriteText MultiplierText => base.MultiplierText;
        }
    }
}
