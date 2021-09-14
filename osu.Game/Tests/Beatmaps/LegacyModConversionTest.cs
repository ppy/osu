// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;

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

        protected void TestFromLegacy(LegacyMods legacyMods, Type[] expectedMods)
        {
            var ruleset = CreateRuleset();
            var mods = ruleset.ConvertFromLegacyMods(legacyMods).ToList();
            Assert.AreEqual(expectedMods.Length, mods.Count);

            foreach (var modType in expectedMods)
            {
                Assert.IsNotNull(mods.SingleOrDefault(mod => mod.GetType() == modType));
            }
        }

        protected void TestToLegacy(LegacyMods expectedLegacyMods, Type[] providedModTypes)
        {
            var ruleset = CreateRuleset();
            var modInstances = ruleset.CreateAllMods()
                                      .Where(mod => providedModTypes.Contains(mod.GetType()))
                                      .ToArray();
            var actualLegacyMods = ruleset.ConvertToLegacyMods(modInstances);
            Assert.AreEqual(expectedLegacyMods, actualLegacyMods);
        }
    }
}
