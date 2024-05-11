// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
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

        private readonly Dictionary<HitResult, JudgementCount> results = new Dictionary<HitResult, JudgementCount>();

        public IEnumerable<JudgementCount> Counters => counters;

        private readonly List<JudgementCount> counters = new List<JudgementCount>();

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            // Due to weirdness in judgements, some results have the same name and should be aggregated for display purposes.
            // There's only one case of this right now ("slider end").
            foreach (var group in ruleset.Value.CreateInstance().GetHitResults().GroupBy(r => r.displayName))
            {
                var judgementCount = new JudgementCount
                {
                    DisplayName = group.Key,
                    Types = group.Select(r => r.result).ToArray(),
                    ResultCount = new BindableInt()
                };

                counters.Add(judgementCount);

                foreach (var r in group)
                    results[r.result] = judgementCount;
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
            if (!results.TryGetValue(judgement.Type, out var count))
                return;

            if (revert)
                count.ResultCount.Value--;
            else
                count.ResultCount.Value++;
        }

        public struct JudgementCount
        {
            public LocalisableString DisplayName { get; set; }

            public HitResult[] Types { get; set; }

            public BindableInt ResultCount { get; set; }
        }
    }
}
