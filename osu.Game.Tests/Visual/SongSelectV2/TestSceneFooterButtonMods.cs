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
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.SelectV2;
using osu.Game.Utils;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneFooterButtonMods : OsuTestScene
    {
        private readonly FooterButtonMods footerButtonMods;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public TestSceneFooterButtonMods()
        {
            Add(footerButtonMods = new FooterButtonMods(new TestModSelectOverlay())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.CentreLeft,
                Action = () => { },
                X = -100,
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
            AddStep("modified + five", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } }, new OsuModClassic(), new OsuModDifficultyAdjust(), new OsuModRandom() }));
            AddStep("modified + six", () => changeMods(new List<Mod> { new OsuModHidden(), new OsuModHardRock(), new OsuModDoubleTime { SpeedChange = { Value = 1.2 } }, new OsuModClassic(), new OsuModDifficultyAdjust(), new OsuModRandom(), new OsuModAlternate() }));

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
            assertModsMultiplier(hiddenMod);

            var hardRockMod = new Mod[] { new OsuModHardRock() };
            AddStep(@"Add HardRock", () => changeMods(hardRockMod));
            assertModsMultiplier(hardRockMod);

            var doubleTimeMod = new Mod[] { new OsuModDoubleTime() };
            AddStep(@"Add DoubleTime", () => changeMods(doubleTimeMod));
            assertModsMultiplier(doubleTimeMod);

            var multipleIncrementMods = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModHardRock() };
            AddStep(@"Add multiple Mods", () => changeMods(multipleIncrementMods));
            assertModsMultiplier(multipleIncrementMods);
        }

        [Test]
        public void TestDecrementMultiplier()
        {
            var easyMod = new Mod[] { new OsuModEasy() };
            AddStep(@"Add Easy", () => changeMods(easyMod));
            assertModsMultiplier(easyMod);

            var noFailMod = new Mod[] { new OsuModNoFail() };
            AddStep(@"Add NoFail", () => changeMods(noFailMod));
            assertModsMultiplier(noFailMod);

            var multipleDecrementMods = new Mod[] { new OsuModEasy(), new OsuModNoFail() };
            AddStep(@"Add Multiple Mods", () => changeMods(multipleDecrementMods));
            assertModsMultiplier(multipleDecrementMods);
        }

        [Test]
        public void TestUnrankedBadge()
        {
            AddStep(@"Add unranked mod", () => changeMods(new[] { new OsuModDeflate() }));
            AddUntilStep("Unranked badge shown", () => footerButtonMods.ChildrenOfType<FooterButtonMods.UnrankedBadge>().Single().Alpha == 1);
            AddStep(@"Clear selected mod", () => changeMods(Array.Empty<Mod>()));
            AddUntilStep("Unranked badge not shown", () => footerButtonMods.ChildrenOfType<FooterButtonMods.UnrankedBadge>().Single().Alpha == 0);
        }

        private void changeMods(IReadOnlyList<Mod> mods) => footerButtonMods.Current.Value = mods;

        private void assertModsMultiplier(IEnumerable<Mod> mods)
        {
            double multiplier = mods.Aggregate(1.0, (current, mod) => current * mod.ScoreMultiplier);
            string expectedValue = ModUtils.FormatScoreMultiplier(multiplier).ToString();

            AddAssert($"Displayed multiplier is {expectedValue}", () => footerButtonMods.ChildrenOfType<OsuSpriteText>().First(t => t.Text.ToString().Contains('x')).Text.ToString(), () => Is.EqualTo(expectedValue));
        }

        private partial class TestModSelectOverlay : UserModSelectOverlay
        {
            public TestModSelectOverlay()
                : base(OverlayColourScheme.Aquamarine)
            {
                ShowPresets = true;
            }
        }
    }
}
