// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest]
    public class TestSceneNoConflictingModAcronyms : TestSceneAllRulesetPlayers
    {
        protected override void AddCheckSteps()
        {
            AddStep("Check all mod acronyms are unique", () =>
            {
                var mods = Ruleset.Value.CreateInstance().AllMods;

                IEnumerable<string> acronyms = mods.Select(m => m.Acronym);

                Assert.That(acronyms, Is.Unique);
            });
        }
    }
}
