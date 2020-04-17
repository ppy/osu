// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public abstract class OsuSkinnableTestScene : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuRuleset),
            typeof(OsuLegacySkinTransformer),
        };

        protected override Ruleset CreateRuleset() => new OsuRuleset();
    }
}
