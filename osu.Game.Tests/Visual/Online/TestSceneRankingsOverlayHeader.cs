// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Rankings;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsOverlayHeader : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankingsOverlayHeader),
        };

        public TestSceneRankingsOverlayHeader()
        {
            var ruleset = new Bindable<RulesetInfo>();
            var scope = new Bindable<RankingsScope>();

            Add(new RankingsOverlayHeader
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Ruleset = { BindTarget = ruleset },
                Current = { BindTarget = scope }
            });

            AddStep("Set scope to Score", () => scope.Value = RankingsScope.Score);
            AddStep("Set scope to Country", () => scope.Value = RankingsScope.Country);
            AddStep("Set scope to Performance", () => scope.Value = RankingsScope.Performance);
            AddStep("Set scope to Spotlights", () => scope.Value = RankingsScope.Spotlights);
        }
    }
}
