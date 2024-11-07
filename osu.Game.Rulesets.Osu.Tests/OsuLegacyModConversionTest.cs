// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private static readonly object[][] osu_mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { typeof(OsuModNoFail) } },
            new object[] { LegacyMods.Easy, new[] { typeof(OsuModEasy) } },
            new object[] { LegacyMods.TouchDevice, new[] { typeof(OsuModTouchDevice) } },
            new object[] { LegacyMods.Hidden, new[] { typeof(OsuModHidden) } },
            new object[] { LegacyMods.HardRock, new[] { typeof(OsuModHardRock) } },
            new object[] { LegacyMods.SuddenDeath, new[] { typeof(OsuModSuddenDeath) } },
            new object[] { LegacyMods.DoubleTime, new[] { typeof(OsuModDoubleTime) } },
            new object[] { LegacyMods.Relax, new[] { typeof(OsuModRelax) } },
            new object[] { LegacyMods.HalfTime, new[] { typeof(OsuModHalfTime) } },
            new object[] { LegacyMods.Flashlight, new[] { typeof(OsuModFlashlight) } },
            new object[] { LegacyMods.Autoplay, new[] { typeof(OsuModAutoplay) } },
            new object[] { LegacyMods.SpunOut, new[] { typeof(OsuModSpunOut) } },
            new object[] { LegacyMods.Autopilot, new[] { typeof(OsuModAutopilot) } },
            new object[] { LegacyMods.Target, new[] { typeof(OsuModTargetPractice) } },
            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(OsuModHardRock), typeof(OsuModDoubleTime) } },
            new object[] { LegacyMods.ScoreV2, new[] { typeof(ModScoreV2) } },
        };

        [TestCaseSource(nameof(osu_mod_mapping))]
        [TestCase(LegacyMods.Cinema, new[] { typeof(OsuModCinema) })]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(OsuModCinema) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(OsuModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(OsuModNightcore) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(OsuModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(OsuModPerfect) })]
        public new void TestFromLegacy(LegacyMods legacyMods, Type[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(osu_mod_mapping))]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(OsuModCinema) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(OsuModNightcore) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(OsuModPerfect) })]
        public new void TestToLegacy(LegacyMods legacyMods, Type[] givenMods) => base.TestToLegacy(legacyMods, givenMods);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
