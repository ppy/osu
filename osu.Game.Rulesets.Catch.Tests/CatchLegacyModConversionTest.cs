// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { new CatchModNoFail() } },
            new object[] { LegacyMods.Easy, new[] { new CatchModEasy() } },
            new object[] { LegacyMods.Hidden, new[] { new CatchModHidden() } },
            new object[] { LegacyMods.HardRock, new[] { new CatchModHardRock() } },
            new object[] { LegacyMods.SuddenDeath, new[] { new CatchModSuddenDeath() } },
            new object[] { LegacyMods.DoubleTime, new[] { new CatchModDoubleTime() } },
            new object[] { LegacyMods.Relax, new[] { new CatchModRelax() } },
            new object[] { LegacyMods.HalfTime, new[] { new CatchModHalfTime() } },
            new object[] { LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { new CatchModNightcore() } },
            new object[] { LegacyMods.Flashlight, new[] { new CatchModFlashlight() } },
            new object[] { LegacyMods.Autoplay, new[] { new CatchModAutoplay() } },
            new object[] { LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { new CatchModPerfect() } },
            new object[] { LegacyMods.Cinema | LegacyMods.Autoplay, new[] { new CatchModCinema() } },
            new object[] { LegacyMods.ScoreV2, new[] { new ModScoreV2() } },

            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new Mod[] { new CatchModHardRock(), new CatchModDoubleTime() } },
        };

        private static readonly object[][] mod_mapping_from_legacy =
        {
            new object[] { LegacyMods.Nightcore, new[] { new CatchModNightcore() } },
            new object[] { LegacyMods.Perfect, new[] { new CatchModPerfect() } },
            new object[] { LegacyMods.Cinema, new[] { new CatchModCinema() } },
        };

        [TestCaseSource(nameof(mod_mapping))]
        [TestCaseSource(nameof(mod_mapping_from_legacy))]
        public new void TestFromLegacy(LegacyMods legacyMods, Mod[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(mod_mapping))]
        public new void TestToLegacy(LegacyMods expectedLegacyMods, Mod[] mods) => base.TestToLegacy(expectedLegacyMods, mods);

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }
}
