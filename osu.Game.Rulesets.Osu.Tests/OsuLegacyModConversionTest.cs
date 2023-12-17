// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { new OsuModNoFail() } },
            new object[] { LegacyMods.Easy, new[] { new OsuModEasy() } },
            new object[] { LegacyMods.TouchDevice, new[] { new OsuModTouchDevice() } },
            new object[] { LegacyMods.Hidden, new[] { new OsuModHidden() } },
            new object[] { LegacyMods.HardRock, new[] { new OsuModHardRock() } },
            new object[] { LegacyMods.SuddenDeath, new[] { new OsuModSuddenDeath() } },
            new object[] { LegacyMods.DoubleTime, new[] { new OsuModDoubleTime() } },
            new object[] { LegacyMods.Relax, new[] { new OsuModRelax() } },
            new object[] { LegacyMods.HalfTime, new[] { new OsuModHalfTime() } },
            new object[] { LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { new OsuModNightcore() } },
            new object[] { LegacyMods.Flashlight, new[] { new OsuModFlashlight() } },
            new object[] { LegacyMods.Autoplay, new[] { new OsuModAutoplay() } },
            new object[] { LegacyMods.SpunOut, new[] { new OsuModSpunOut() } },
            new object[] { LegacyMods.Autopilot, new[] { new OsuModAutopilot() } },
            new object[] { LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { new OsuModPerfect() } },
            new object[] { LegacyMods.Cinema | LegacyMods.Autoplay, new[] { new OsuModCinema() } },
            new object[] { LegacyMods.Target, new[] { new OsuModTargetPractice() } },
            new object[] { LegacyMods.ScoreV2, new[] { new ModScoreV2() } },

            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() } },
        };

        private static readonly object[][] mod_mapping_from_legacy =
        {
            new object[] { LegacyMods.Nightcore, new[] { new OsuModNightcore() } },
            new object[] { LegacyMods.Perfect, new[] { new OsuModPerfect() } },
            new object[] { LegacyMods.Cinema, new[] { new OsuModCinema() } },
        };

        [TestCaseSource(nameof(mod_mapping))]
        [TestCaseSource(nameof(mod_mapping_from_legacy))]
        public new void TestFromLegacy(LegacyMods legacyMods, Mod[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(mod_mapping))]
        public new void TestToLegacy(LegacyMods expectedLegacyMods, Mod[] mods) => base.TestToLegacy(expectedLegacyMods, mods);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
