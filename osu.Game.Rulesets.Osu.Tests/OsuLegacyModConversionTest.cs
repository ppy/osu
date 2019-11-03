// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuLegacyModConversionTest : LegacyModConversionTest
    {
        [TestCase(LegacyMods.Easy, new[] { typeof(OsuModEasy) })]
        [TestCase(LegacyMods.HardRock | LegacyMods.DoubleTime, new[] { typeof(OsuModHardRock), typeof(OsuModDoubleTime) })]
        [TestCase(LegacyMods.DoubleTime, new[] { typeof(OsuModDoubleTime) })]
        [TestCase(LegacyMods.Nightcore, new[] { typeof(OsuModNightcore) })]
        [TestCase(LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(OsuModNightcore) })]
        [TestCase(LegacyMods.Flashlight | LegacyMods.Nightcore | LegacyMods.DoubleTime, new[] { typeof(OsuModFlashlight), typeof(OsuModFlashlight) })]
        [TestCase(LegacyMods.Perfect, new[] { typeof(OsuModPerfect) })]
        [TestCase(LegacyMods.SuddenDeath, new[] { typeof(OsuModSuddenDeath) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath, new[] { typeof(OsuModPerfect) })]
        [TestCase(LegacyMods.Perfect | LegacyMods.SuddenDeath | LegacyMods.DoubleTime, new[] { typeof(OsuModDoubleTime), typeof(OsuModPerfect) })]
        [TestCase(LegacyMods.SpunOut | LegacyMods.Easy, new[] { typeof(OsuModSpunOut), typeof(OsuModEasy) })]
        public new void Test(LegacyMods legacyMods, Type[] expectedMods) => base.Test(legacyMods, expectedMods);

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
