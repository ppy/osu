// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class CatchLegacyModConversionTest : LegacyModConversionTest
    {
        private static readonly object[][] catch_mod_mapping =
        {
            new object[] { LegacyMods.NoFail, new[] { typeof(CatchModNoFail) } },
            new object[] { LegacyMods.Easy, new[] { typeof(CatchModEasy) } },
            new object[] { LegacyMods.Hidden, new[] { typeof(CatchModHidden) } },
            new object[] { LegacyMods.HardRock, new[] { typeof(CatchModHardRock) } },
            new object[] { LegacyMods.SuddenDeath, new[] { typeof(CatchModSuddenDeath) } },
            new object[] { LegacyMods.DoubleTime, new[] { typeof(CatchModDoubleTime) } },
            new object[] { LegacyMods.Relax, new[] { typeof(CatchModRelax) } },
            new object[] { LegacyMods.HalfTime, new[] { typeof(CatchModHalfTime) } },
            new object[] { LegacyMods.Nightcore, new[] { typeof(CatchModNightcore) } },
            new object[] { LegacyMods.Flashlight, new[] { typeof(CatchModFlashlight) } },
            new object[] { LegacyMods.Autoplay, new[] { typeof(CatchModAutoplay) } },
            new object[] { LegacyMods.Perfect, new[] { typeof(CatchModPerfect) } },
            new object[] { LegacyMods.Cinema, new[] { typeof(CatchModCinema) } },
            new object[] { LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(CatchModHardRock), typeof(CatchModDoubleTime) } }
        };

        [TestCaseSource(nameof(catch_mod_mapping))]
        [TestCase(LegacyMods.Cinema | LegacyMods.Autoplay, new[] { typeof(CatchModCinema) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(CatchModNightcore) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(CatchModPerfect) })]
        public new void TestFromLegacy(LegacyMods legacyMods, Type[] expectedMods) => base.TestFromLegacy(legacyMods, expectedMods);

        [TestCaseSource(nameof(catch_mod_mapping))]
        public new void TestToLegacy(LegacyMods legacyMods, Type[] givenMods) => base.TestToLegacy(legacyMods, givenMods);

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }
}
