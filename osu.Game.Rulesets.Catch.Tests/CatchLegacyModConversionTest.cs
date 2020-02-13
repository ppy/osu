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
        [TestCase(LegacyMods.Easy, new[] { typeof(CatchModEasy) })]
        [TestCase(LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(CatchModHardRock), typeof(CatchModDoubleTime) })]
        [TestCase(LegacyMods.DoubleTime, new[] { typeof(CatchModDoubleTime) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(CatchModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(CatchModNightcore) })]
        [TestCase(LegacyMods.Flashlight | LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(CatchModFlashlight), typeof(CatchModNightcore) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(CatchModPerfect) })]
        [TestCase(LegacyMods.SuddenDeath, new[] { typeof(CatchModSuddenDeath) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(CatchModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath | LegacyMods.DoubleTime, new[] { typeof(CatchModDoubleTime), typeof(CatchModPerfect) })]
        public new void Test(LegacyMods legacyMods, Type[] expectedMods) => base.Test(legacyMods, expectedMods);

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }
}
