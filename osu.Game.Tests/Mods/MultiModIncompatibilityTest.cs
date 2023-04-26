// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Utils;

namespace osu.Game.Tests.Mods
{
    [TestFixture]
    public class MultiModIncompatibilityTest
    {
        /// <summary>
        /// Ensures that all mods grouped into <see cref="MultiMod"/>s, as declared by the default rulesets, are pairwise incompatible with each other.
        /// </summary>
        [TestCase(typeof(OsuRuleset))]
        [TestCase(typeof(TaikoRuleset))]
        [TestCase(typeof(CatchRuleset))]
        [TestCase(typeof(ManiaRuleset))]
        public void TestAllMultiModsFromRulesetAreIncompatible(Type rulesetType)
        {
            var ruleset = Activator.CreateInstance(rulesetType) as Ruleset;
            Assert.That(ruleset, Is.Not.Null);

            var allMultiMods = getMultiMods(ruleset!);

            Assert.Multiple(() =>
            {
                foreach (var multiMod in allMultiMods)
                {
                    int modCount = multiMod.Mods.Length;

                    for (int i = 0; i < modCount; ++i)
                    {
                        // indexing from i + 1 ensures that only pairs of different mods are checked, and are checked only once
                        // (indexing from 0 would check each pair twice, and also check each mod against itself).
                        for (int j = i + 1; j < modCount; ++j)
                        {
                            var firstMod = multiMod.Mods[i];
                            var secondMod = multiMod.Mods[j];

                            Assert.That(
                                ModUtils.CheckCompatibleSet(new[] { firstMod, secondMod }), Is.False,
                                $"{firstMod.Name} ({firstMod.Acronym}) and {secondMod.Name} ({secondMod.Acronym}) should be incompatible.");
                        }
                    }
                }
            });
        }

        /// <remarks>
        /// This local helper is used rather than <see cref="Ruleset.CreateAllMods"/>, because the aforementioned method flattens multi mods.
        /// </remarks>>
        private static IEnumerable<MultiMod> getMultiMods(Ruleset ruleset)
            => Enum.GetValues<ModType>().SelectMany(ruleset.GetModsFor).OfType<MultiMod>();
    }
}
