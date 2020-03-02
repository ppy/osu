// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestSceneTaikoModSandbox : ModSandboxTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Append(typeof(TestSceneTaikoModSandbox)).ToList();

        public TestSceneTaikoModSandbox()
            : this(null)
        {
        }

        public TestSceneTaikoModSandbox(Mod mod = null)
            : base(new TaikoRuleset(), mod)
        {
        }
    }
}
