// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Taiko.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public abstract class TaikoSkinnableTestScene : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TaikoRuleset),
            typeof(TaikoLegacySkinTransformer),
        };

        protected override Ruleset CreateRulesetForSkinProvider() => new TaikoRuleset();
    }
}
