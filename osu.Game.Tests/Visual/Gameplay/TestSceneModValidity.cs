// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest]
    public class TestSceneModValidity : TestSceneAllRulesetPlayers
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

                foreach (var mod in modInstances)
                {
                    var modIncompatibilities = mod.IncompatibleMods;

                    foreach (var incompatibleModType in modIncompatibilities)
                    {
                        var incompatibleMod = modInstances.First(m => incompatibleModType.IsInstanceOfType(m));
                        Assert.That(incompatibleMod.IncompatibleMods.Contains(mod.GetType()));
                    }
                }
            });
        }
    }
}
