// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatchModSandbox : ModSandboxTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Append(typeof(TestSceneCatchModSandbox)).ToList();

        public TestSceneCatchModSandbox()
            : this(null)
        {
        }

        public TestSceneCatchModSandbox(Mod mod = null)
            : base(new CatchRuleset(), mod)
        {
        }
    }
}
