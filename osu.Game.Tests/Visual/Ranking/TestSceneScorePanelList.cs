// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanelList : OsuTestScene
    {
        public TestSceneScorePanelList()
        {
            var list = new ScorePanelList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            Add(list);

            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
            list.AddScore(new TestScoreInfo(new OsuRuleset().RulesetInfo));
        }
    }
}
