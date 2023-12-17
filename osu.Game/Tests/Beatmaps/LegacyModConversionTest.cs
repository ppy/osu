// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Beatmaps
{
    /// <summary>
    /// Base class for tests of converting <see cref="LegacyMods"/> enumeration flags to ruleset mod instances.
    /// </summary>
    public abstract class LegacyModConversionTest
    {
        /// <summary>
        /// Creates the <see cref="Ruleset"/> whose legacy mod conversion is to be tested.
        /// </summary>
        protected abstract Ruleset CreateRuleset();

        protected void TestFromLegacy(LegacyMods legacyMods, Mod[] expectedMods)
        {
            var ruleset = CreateRuleset();
            var actualMods = ruleset.ConvertFromLegacyMods(legacyMods).ToList();
            Assert.AreEqual(expectedMods.Length, actualMods.Count);

            foreach (var expectedMod in expectedMods)
            {
                Assert.Contains(expectedMod, actualMods);
            }
        }

        protected void TestToLegacy(LegacyMods expectedLegacyMods, Mod[] mods)
        {
            var ruleset = CreateRuleset();
            var actualLegacyMods = ruleset.ConvertToLegacyMods(mods);
            Assert.AreEqual(expectedLegacyMods, actualLegacyMods);
        }
    }
}
