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
        [TestCase(LegacyMods.Easy, new[] { typeof(ManiaModEasy) })]
        [TestCase(LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(ManiaModHardRock), typeof(ManiaModDoubleTime) })]
        [TestCase(LegacyMods.DoubleTime, new[] { typeof(ManiaModDoubleTime) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(ManiaModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(ManiaModNightcore) })]
        [TestCase(LegacyMods.Flashlight | LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(ManiaModFlashlight), typeof(ManiaModNightcore) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(ManiaModPerfect) })]
        [TestCase(LegacyMods.SuddenDeath, new[] { typeof(ManiaModSuddenDeath) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(ManiaModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath | LegacyMods.DoubleTime, new[] { typeof(ManiaModDoubleTime), typeof(ManiaModPerfect) })]
        [TestCase(LegacyMods.Random | LegacyMods.SuddenDeath, new[] { typeof(ManiaModRandom), typeof(ManiaModSuddenDeath) })]
        public new void Test(LegacyMods legacyMods, Type[] expectedMods) => base.Test(legacyMods, expectedMods);

        protected override Ruleset CreateRuleset() => new ManiaRuleset();
    }
}
