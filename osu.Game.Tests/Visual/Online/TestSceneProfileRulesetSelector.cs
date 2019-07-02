// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays.Profile.Header.Components;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneProfileRulesetSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ProfileRulesetSelector),
            typeof(ProfileRulesetTabItem),
        };

        public TestSceneProfileRulesetSelector()
        {
            ProfileRulesetSelector selector;

            Child = selector = new ProfileRulesetSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            AddStep("set osu! as default", () => selector.SetDefaultRuleset(new OsuRuleset().RulesetInfo));
            AddStep("set mania as default", () => selector.SetDefaultRuleset(new ManiaRuleset().RulesetInfo));
            AddStep("set taiko as default", () => selector.SetDefaultRuleset(new TaikoRuleset().RulesetInfo));
            AddStep("set catch as default", () => selector.SetDefaultRuleset(new CatchRuleset().RulesetInfo));
            AddStep("select default ruleset", selector.SelectDefaultRuleset);
        }
    }
}
