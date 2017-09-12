// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Scoring
{
    internal class OsuScoreProcessor : ScoreProcessor<OsuHitObject>
    {
        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>(ScoringMode.Exponential);

        protected override double ComboPortion => 0.7;
        protected override double AccuracyPortion => 0.3;

        public OsuScoreProcessor()
        {
        }

        public OsuScoreProcessor(RulesetContainer<OsuHitObject> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        private float hpDrainRate;

        private readonly Dictionary<HitResult, int> scoreResultCounts = new Dictionary<HitResult, int>();
        private readonly Dictionary<ComboResult, int> comboResultCounts = new Dictionary<ComboResult, int>();

        protected override void ComputeTargets(Beatmap<OsuHitObject> beatmap)
        {
            hpDrainRate = beatmap.BeatmapInfo.Difficulty.DrainRate;

            foreach (var unused in beatmap.HitObjects)
            {
                // TODO: add support for other object types.
                AddJudgement(new OsuJudgement { Result = HitResult.Great });
            }
        }

        protected override void Reset()
        {
            base.Reset();

            scoreResultCounts.Clear();
            comboResultCounts.Clear();
        }

        public override void PopulateScore(Score score)
        {
            base.PopulateScore(score);

            score.Statistics[@"300"] = scoreResultCounts.GetOrDefault(HitResult.Great);
            score.Statistics[@"100"] = scoreResultCounts.GetOrDefault(HitResult.Good);
            score.Statistics[@"50"] = scoreResultCounts.GetOrDefault(HitResult.Meh);
            score.Statistics[@"x"] = scoreResultCounts.GetOrDefault(HitResult.Miss);
        }

        protected override void OnNewJudgement(Judgement judgement)
        {
            base.OnNewJudgement(judgement);

            var osuJudgement = (OsuJudgement)judgement;

            if (judgement.Result != HitResult.None)
            {
                scoreResultCounts[judgement.Result] = scoreResultCounts.GetOrDefault(judgement.Result) + 1;
                comboResultCounts[osuJudgement.Combo] = comboResultCounts.GetOrDefault(osuJudgement.Combo) + 1;
            }

            switch (judgement.Result)
            {
                case HitResult.Great:
                    Health.Value += (10.2 - hpDrainRate) * 0.02;
                    break;

                case HitResult.Good:
                    Health.Value += (8 - hpDrainRate) * 0.02;
                    break;

                case HitResult.Meh:
                    Health.Value += (4 - hpDrainRate) * 0.02;
                    break;

                /*case HitResult.SliderTick:
                    Health.Value += Math.Max(7 - hpDrainRate, 0) * 0.01;
                    break;*/

                case HitResult.Miss:
                    Health.Value -= hpDrainRate * 0.04;
                    break;
            }
        }

        public enum ScoringMode
        {
            Standardised,
            Exponential
        }
    }
}
