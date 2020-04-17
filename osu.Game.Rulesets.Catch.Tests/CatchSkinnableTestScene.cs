// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Catch.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    public abstract class CatchSkinnableTestScene : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(CatchRuleset),
            typeof(CatchLegacySkinTransformer),
        };

        protected override Ruleset CreateRuleset() => new CatchRuleset();
    }
}
