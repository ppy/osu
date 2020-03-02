// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mods;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneOsuModSandbox : ModSandboxTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Append(typeof(TestSceneOsuModSandbox)).ToList();

        public TestSceneOsuModSandbox()
            : this(null)
        {
        }

        public TestSceneOsuModSandbox(Mod mod = null)
            : base(new OsuRuleset(), mod)
        {
        }
    }
}
