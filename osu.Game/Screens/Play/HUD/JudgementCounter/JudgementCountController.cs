// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD.JudgementCounter
{
    /// <summary>
    /// Keeps track of judgements for a current play session, exposing bindable counts which can
    /// be used for display purposes.
    /// </summary>
    public partial class JudgementCountController : Component
    {
        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public List<JudgementCount> Results = new List<JudgementCount>();

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            foreach (var result in ruleset.Value.CreateInstance().GetHitResults())
            {
                Results.Add(new JudgementCount
                {
                    Type = result.result,
                    ResultCount = new BindableInt()
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += judgement => updateCount(judgement, false);
            scoreProcessor.JudgementReverted += judgement => updateCount(judgement, true);
        }

        private void updateCount(JudgementResult judgement, bool revert)
        {
            foreach (JudgementCount result in Results.Where(result => result.Type == judgement.Type))
                result.ResultCount.Value = revert ? result.ResultCount.Value - 1 : result.ResultCount.Value + 1;
        }

        public struct JudgementCount
        {
            public HitResult Type { get; set; }

            public BindableInt ResultCount { get; set; }
        }
    }
}
