// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] mania_mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { typeof(ManiaModNoFail) } },
            new object[] { LegacyMods.Easy, new[] { typeof(ManiaModEasy) } },
            new object[] { LegacyMods.Hidden, new[] { typeof(ManiaModHidden) } },
            new object[] { LegacyMods.HardRock, new[] { typeof(ManiaModHardRock) } },
            new object[] { LegacyMods.SuddenDeath, new[] { typeof(ManiaModSuddenDeath) } },
            new object[] { LegacyMods.DoubleTime, new[] { typeof(ManiaModDoubleTime) } },
            new object[] { LegacyMods.HalfTime, new[] { typeof(ManiaModHalfTime) } },
            new object[] { LegacyMods.Nightcore, new[] { typeof(ManiaModNightcore) } },
            new object[] { LegacyMods.Flashlight, new[] { typeof(ManiaModFlashlight) } },
            new object[] { LegacyMods.Autoplay, new[] { typeof(ManiaModAutoplay) } },
            new object[] { LegacyMods.Perfect, new[] { typeof(ManiaModPerfect) } },
            new object[] { LegacyMods.Key4, new[] { typeof(ManiaModKey4) } },
            new object[] { LegacyMods.Key5, new[] { typeof(ManiaModKey5) } },
            new object[] { LegacyMods.Key6, new[] { typeof(ManiaModKey6) } },
            new object[] { LegacyMods.Key7, new[] { typeof(ManiaModKey7) } },
            new object[] { LegacyMods.Key8, new[] { typeof(ManiaModKey8) } },
            new object[] { LegacyMods.FadeIn, new[] { typeof(ManiaModFadeIn) } },
            new object[] { LegacyMods.Random, new[] { typeof(ManiaModRandom) } },
            new object[] { LegacyMods.Cinema, new[] { typeof(ManiaModCinema) } },
            new object[] { LegacyMods.Key9, new[] { typeof(ManiaModKey9) } },
            new object[] { LegacyMods.KeyCoop, new[] { typeof(ManiaModDualStages) } },
            new object[] { LegacyMods.Key1, new[] { typeof(ManiaModKey1) } },
            new object[] { LegacyMods.Key3, new[] { typeof(ManiaModKey3) } },
            new object[] { LegacyMods.Key2, new[] { typeof(ManiaModKey2) } },
            new object[] { LegacyMods.Mirror, new[] { typeof(ManiaModMirror) } },
            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(ManiaModHardRock), typeof(ManiaModDoubleTime) } }
        };

        [TestCaseSource(nameof(mania_mod_mapping))]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(ManiaModCinema) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(ManiaModNightcore) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(ManiaModPerfect) })]
        public new void TestFromLegacy(LegacyMods legacyMods, Type[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(mania_mod_mapping))]
        public new void TestToLegacy(LegacyMods legacyMods, Type[] givenMods) => base.TestToLegacy(legacyMods, givenMods);

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }
}
