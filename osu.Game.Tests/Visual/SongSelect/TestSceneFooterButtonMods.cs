// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select;
using osu.Game.Utils;

namespace osu.Game.Tests.Visual.SongSelect
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
            var ruleset = new OsuRuleset();
            var hiddenMod = new Mod[] { new OsuModHidden() };

            AddStep("Set ruleset", () => footerButtonMods.Ruleset.Value = ruleset.RulesetInfo);

            AddStep(@"Add Hidden", () => changeMods(hiddenMod));
            assertModsMultiplier(1.04);

            var hardRockMod = new Mod[] { new OsuModHardRock() };
            AddStep(@"Add HardRock", () => changeMods(hardRockMod));
            assertModsMultiplier(1.09);

            var doubleTimeMod = new Mod[] { new OsuModDoubleTime() };
            AddStep(@"Add DoubleTime", () => changeMods(doubleTimeMod));
            assertModsMultiplier(1.23);

            var multipleIncrementMods = new Mod[] { new OsuModDoubleTime(), new OsuModHidden(), new OsuModHardRock() };
            AddStep(@"Add multiple Mods", () => changeMods(multipleIncrementMods));
            assertModsMultiplier(1.23 * 1.04 * 1.09);
        }

        [Test]
        public void TestDecrementMultiplier()
        {
            var ruleset = new OsuRuleset();
            var easyMod = new Mod[] { new OsuModEasy() };

            AddStep("Set ruleset", () => footerButtonMods.Ruleset.Value = ruleset.RulesetInfo);

            AddStep(@"Add Easy", () => changeMods(easyMod));
            assertModsMultiplier(0.8);

            var noFailMod = new Mod[] { new OsuModNoFail() };
            AddStep(@"Add NoFail", () => changeMods(noFailMod));
            assertModsMultiplier(0.5);

            var multipleDecrementMods = new Mod[] { new OsuModEasy(), new OsuModNoFail() };
            AddStep(@"Add Multiple Mods", () => changeMods(multipleDecrementMods));
            assertModsMultiplier(0.8 * 0.5);
        }

        [Test]
        public void TestDifficultyAdjustMultiplier()
        {
            var ruleset = new OsuRuleset();
            var difficultyAdjustMod = new OsuModDifficultyAdjust();

            AddStep("Set ruleset", () => footerButtonMods.Ruleset.Value = ruleset.RulesetInfo);

            AddStep("Set beatmap", () =>
            {
                var beatmap = CreateWorkingBeatmap(ruleset.RulesetInfo);
                beatmap.BeatmapInfo.Difficulty = new BeatmapDifficulty
                {
                    ApproachRate = 3,
                    OverallDifficulty = 5,
                    CircleSize = 5,
                    DrainRate = 6,
                };
                Beatmap.Value = beatmap;
            });

            AddStep(@"Set Difficulty Adjust", () => changeMods([difficultyAdjustMod]));
            assertModsMultiplier(1);

            AddStep("Adjust AR", () => difficultyAdjustMod.ApproachRate.Value = 3.3f);
            assertModsMultiplier(0.85);

            AddStep("Adjust HP", () => difficultyAdjustMod.DrainRate.Value = 6.5f);
            assertModsMultiplier(0.6375);

            AddStep("Change beatmap", () =>
            {
                var beatmap = CreateWorkingBeatmap(ruleset.RulesetInfo);
                beatmap.BeatmapInfo.Difficulty = new BeatmapDifficulty
                {
                    ApproachRate = 3.3f,
                    OverallDifficulty = 8,
                    CircleSize = 8,
                    DrainRate = 6,
                };
                Beatmap.Value = beatmap;
            });
            assertModsMultiplier(0.75);
        }

        [Test]
        public void TestUnrankedBadge()
        {
            AddStep(@"Add unranked mod", () => changeMods(new[] { new OsuModDeflate() }));
            AddUntilStep("Unranked badge shown", () => footerButtonMods.ChildrenOfType<FooterButtonMods.UnrankedBadge>().Single().Alpha == 1);
            AddStep(@"Clear selected mod", () => changeMods(Array.Empty<Mod>()));
            AddUntilStep("Unranked badge not shown", () => footerButtonMods.ChildrenOfType<FooterButtonMods.UnrankedBadge>().Single().Alpha == 0);
        }

        private void changeMods(IReadOnlyList<Mod> mods) => footerButtonMods.Mods.Value = mods;

        private void assertModsMultiplier(double expectedMultiplier)
        {
            string expectedValue = ModUtils.FormatScoreMultiplier(expectedMultiplier).ToString();

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
