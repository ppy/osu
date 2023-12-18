// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { new ManiaModNoFail() } },
            new object[] { LegacyMods.Easy, new[] { new ManiaModEasy() } },
            new object[] { LegacyMods.Hidden, new[] { new ManiaModHidden() } },
            new object[] { LegacyMods.HardRock, new[] { new ManiaModHardRock() } },
            new object[] { LegacyMods.SuddenDeath, new[] { new ManiaModSuddenDeath() } },
            new object[] { LegacyMods.DoubleTime, new[] { new ManiaModDoubleTime() } },
            new object[] { LegacyMods.HalfTime, new[] { new ManiaModHalfTime() } },
            new object[] { LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { new ManiaModNightcore() } },
            new object[] { LegacyMods.Flashlight, new[] { new ManiaModFlashlight() } },
            new object[] { LegacyMods.Autoplay, new[] { new ManiaModAutoplay() } },
            new object[] { LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { new ManiaModPerfect() } },
            new object[] { LegacyMods.Key4, new[] { new ManiaModKeyCount { KeyCount = { Value = 4 } } } },
            new object[] { LegacyMods.Key5, new[] { new ManiaModKeyCount { KeyCount = { Value = 5 } } } },
            new object[] { LegacyMods.Key6, new[] { new ManiaModKeyCount { KeyCount = { Value = 6 } } } },
            new object[] { LegacyMods.Key7, new[] { new ManiaModKeyCount { KeyCount = { Value = 7 } } } },
            new object[] { LegacyMods.Key8, new[] { new ManiaModKeyCount { KeyCount = { Value = 8 } } } },
            new object[] { LegacyMods.FadeIn, new[] { new ManiaModFadeIn() } },
            new object[] { LegacyMods.Random, new[] { new ManiaModRandom() } },
            new object[] { LegacyMods.Cinema | LegacyMods.Autoplay, new[] { new ManiaModCinema() } },
            new object[] { LegacyMods.Key9, new[] { new ManiaModKeyCount { KeyCount = { Value = 9 } } } },
            new object[] { LegacyMods.KeyCoop, new[] { new ManiaModDualStages() } },
            new object[] { LegacyMods.Key1, new[] { new ManiaModKeyCount { KeyCount = { Value = 1 } } } },
            new object[] { LegacyMods.Key3, new[] { new ManiaModKeyCount { KeyCount = { Value = 3 } } } },
            new object[] { LegacyMods.Key2, new[] { new ManiaModKeyCount { KeyCount = { Value = 2 } } } },
            new object[] { LegacyMods.ScoreV2, new[] { new ModScoreV2() } },
            new object[] { LegacyMods.Mirror, new[] { new ManiaModMirror() } },

            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new Mod[] { new ManiaModHardRock(), new ManiaModDoubleTime() } },
        };

        private static readonly object[][] mod_mapping_from_legacy =
        {
            new object[] { LegacyMods.Nightcore, new[] { new ManiaModNightcore() } },
            new object[] { LegacyMods.Perfect, new[] { new ManiaModPerfect() } },
            new object[] { LegacyMods.Cinema, new[] { new ManiaModCinema() } },
        };

        [TestCaseSource(nameof(mod_mapping))]
        [TestCaseSource(nameof(mod_mapping_from_legacy))]
        public new void TestFromLegacy(LegacyMods legacyMods, Mod[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(mod_mapping))]
        public new void TestToLegacy(LegacyMods expectedLegacyMods, Mod[] mods) => base.TestToLegacy(expectedLegacyMods, mods);

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }
}
