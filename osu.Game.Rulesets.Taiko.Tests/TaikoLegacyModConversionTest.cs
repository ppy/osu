// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TaikoLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { new TaikoModNoFail() } },
            new object[] { LegacyMods.Easy, new[] { new TaikoModEasy() } },
            new object[] { LegacyMods.Hidden, new[] { new TaikoModHidden() } },
            new object[] { LegacyMods.HardRock, new[] { new TaikoModHardRock() } },
            new object[] { LegacyMods.SuddenDeath, new[] { new TaikoModSuddenDeath() } },
            new object[] { LegacyMods.DoubleTime, new[] { new TaikoModDoubleTime() } },
            new object[] { LegacyMods.Relax, new[] { new TaikoModRelax() } },
            new object[] { LegacyMods.HalfTime, new[] { new TaikoModHalfTime() } },
            new object[] { LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { new TaikoModNightcore() } },
            new object[] { LegacyMods.Flashlight, new[] { new TaikoModFlashlight() } },
            new object[] { LegacyMods.Autoplay, new[] { new TaikoModAutoplay() } },
            new object[] { LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { new TaikoModPerfect() } },
            new object[] { LegacyMods.Random, new[] { new TaikoModRandom() } },
            new object[] { LegacyMods.Cinema | LegacyMods.Autoplay, new[] { new TaikoModCinema() } },
            new object[] { LegacyMods.ScoreV2, new[] { new ModScoreV2() } },

            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new Mod[] { new TaikoModHardRock(), new TaikoModDoubleTime() } },
        };

        private static readonly object[][] mod_mapping_from_legacy =
        {
            new object[] { LegacyMods.Nightcore, new[] { new TaikoModNightcore() } },
            new object[] { LegacyMods.Perfect, new[] { new TaikoModPerfect() } },
            new object[] { LegacyMods.Cinema, new[] { new TaikoModCinema() } },
        };

        [TestCaseSource(nameof(mod_mapping))]
        [TestCaseSource(nameof(mod_mapping_from_legacy))]
        public new void TestFromLegacy(LegacyMods legacyMods, Mod[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(mod_mapping))]
        public new void TestToLegacy(LegacyMods expectedLegacyMods, Mod[] mods) => base.TestToLegacy(expectedLegacyMods, mods);

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}
