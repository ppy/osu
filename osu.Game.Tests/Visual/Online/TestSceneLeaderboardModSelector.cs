// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.BeatmapSet;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Catch;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneLeaderboardModSelector : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LeaderboardModSelector),
        };

        public TestSceneLeaderboardModSelector()
        {
            LeaderboardModSelector modSelector;

            Add(modSelector = new LeaderboardModSelector
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            AddStep("osu mods", () => modSelector.ResetRuleset(new OsuRuleset().RulesetInfo));
            AddStep("mania mods", () => modSelector.ResetRuleset(new ManiaRuleset().RulesetInfo));
            AddStep("taiko mods", () => modSelector.ResetRuleset(new TaikoRuleset().RulesetInfo));
            AddStep("catch mods", () => modSelector.ResetRuleset(new CatchRuleset().RulesetInfo));
        }
    }
}
