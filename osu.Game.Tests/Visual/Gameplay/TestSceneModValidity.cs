// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneModValidity : TestSceneAllRulesetPlayers
    {
        protected override void AddCheckSteps()
        {
            AddStep("Check all mod acronyms are unique", () =>
            {
                var mods = Ruleset.Value.CreateInstance().AllMods;

                IEnumerable<string> acronyms = mods.Select(m => m.Acronym);

                Assert.That(acronyms, Is.Unique);
            });

            AddStep("Check all mods are two-way incompatible", () =>
            {
                var mods = Ruleset.Value.CreateInstance().AllMods;

                IEnumerable<Mod> modInstances = mods.Select(mod => mod.CreateInstance());

                foreach (var modToCheck in modInstances)
                {
                    var incompatibleMods = modToCheck.IncompatibleMods;

                    foreach (var incompatible in incompatibleMods)
                    {
                        foreach (var incompatibleMod in modInstances.Where(m => incompatible.IsInstanceOfType(m)))
                        {
                            Assert.That(
                                incompatibleMod.IncompatibleMods.Any(m => m.IsInstanceOfType(modToCheck)),
                                $"{modToCheck} has {incompatibleMod} in it's incompatible mods, but {incompatibleMod} does not have {modToCheck} in it's incompatible mods."
                            );
                        }
                    }
                }
            });
        }
    }
}
