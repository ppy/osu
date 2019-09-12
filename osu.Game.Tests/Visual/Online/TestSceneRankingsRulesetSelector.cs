// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Overlays.Rankings;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneRankingsRulesetSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankingsRulesetSelector),
        };

        public TestSceneRankingsRulesetSelector()
        {
            var current = new Bindable<RulesetInfo>();

            Add(new RankingsRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Current = { BindTarget = current }
            });

            AddStep("Select osu!", () => current.Value = new OsuRuleset().RulesetInfo);
            AddStep("Select mania", () => current.Value = new ManiaRuleset().RulesetInfo);
            AddStep("Select taiko", () => current.Value = new TaikoRuleset().RulesetInfo);
            AddStep("Select catch", () => current.Value = new CatchRuleset().RulesetInfo);
        }
    }
}
