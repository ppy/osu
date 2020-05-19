// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TaikoLegacyModConversionTest : LegacyModConversionTest
    {
        [TestCase(LegacyMods.Easy, new[] { typeof(TaikoModEasy) })]
        [TestCase(LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(TaikoModHardRock), typeof(TaikoModDoubleTime) })]
        [TestCase(LegacyMods.DoubleTime, new[] { typeof(TaikoModDoubleTime) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(TaikoModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(TaikoModNightcore) })]
        [TestCase(LegacyMods.Flashlight | LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(TaikoModFlashlight), typeof(TaikoModNightcore) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(TaikoModPerfect) })]
        [TestCase(LegacyMods.SuddenDeath, new[] { typeof(TaikoModSuddenDeath) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(TaikoModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath | LegacyMods.DoubleTime, new[] { typeof(TaikoModDoubleTime), typeof(TaikoModPerfect) })]
        public new void Test(LegacyMods legacyMods, Type[] expectedMods) => base.Test(legacyMods, expectedMods);

        protected override Ruleset CreateRuleset() => new TaikoRuleset();
    }
}
