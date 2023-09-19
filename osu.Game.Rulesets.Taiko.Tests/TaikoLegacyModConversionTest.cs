// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private static readonly object[][] taiko_mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { typeof(TaikoModNoFail) } },
            new object[] { LegacyMods.Easy, new[] { typeof(TaikoModEasy) } },
            new object[] { LegacyMods.Hidden, new[] { typeof(TaikoModHidden) } },
            new object[] { LegacyMods.HardRock, new[] { typeof(TaikoModHardRock) } },
            new object[] { LegacyMods.SuddenDeath, new[] { typeof(TaikoModSuddenDeath) } },
            new object[] { LegacyMods.DoubleTime, new[] { typeof(TaikoModDoubleTime) } },
            new object[] { LegacyMods.Relax, new[] { typeof(TaikoModRelax) } },
            new object[] { LegacyMods.HalfTime, new[] { typeof(TaikoModHalfTime) } },
            new object[] { LegacyMods.Flashlight, new[] { typeof(TaikoModFlashlight) } },
            new object[] { LegacyMods.Autoplay, new[] { typeof(TaikoModAutoplay) } },
            new object[] { LegacyMods.Random, new[] { typeof(TaikoModRandom) } },
            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(TaikoModHardRock), typeof(TaikoModDoubleTime) } },
            new object[] { LegacyMods.ScoreV2, new[] { typeof(ModScoreV2) } },
        };

        [TestCaseSource(nameof(taiko_mod_mapping))]
        [TestCase(LegacyMods.Cinema, new[] { typeof(TaikoModCinema) })]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(TaikoModCinema) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(TaikoModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(TaikoModNightcore) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(TaikoModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(TaikoModPerfect) })]
        public new void TestFromLegacy(LegacyMods legacyMods, Type[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(taiko_mod_mapping))]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(TaikoModCinema) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(TaikoModNightcore) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(TaikoModPerfect) })]
        public new void TestToLegacy(LegacyMods legacyMods, Type[] givenMods) => base.TestToLegacy(legacyMods, givenMods);

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}
