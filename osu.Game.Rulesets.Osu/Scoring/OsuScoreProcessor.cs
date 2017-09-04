// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu.Scoring
{
    internal class OsuScoreProcessor : ScoreProcessor<OsuHitObject, OsuJudgement>
    {
        public OsuScoreProcessor()
        {
        }

        public OsuScoreProcessor(RulesetContainer<OsuHitObject, OsuJudgement> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        private float hpDrainRate;

        protected override void ComputeTargets(Game.Beatmaps.Beatmap<OsuHitObject> beatmap)
        {
            hpDrainRate = beatmap.BeatmapInfo.Difficulty.DrainRate;
        }

        protected override void Reset()
        {
            base.Reset();

            Health.Value = 1;
            Accuracy.Value = 1;

            scoreResultCounts.Clear();
            comboResultCounts.Clear();
        }

        private readonly Dictionary<OsuScoreResult, int> scoreResultCounts = new Dictionary<OsuScoreResult, int>();
        private readonly Dictionary<ComboResult, int> comboResultCounts = new Dictionary<ComboResult, int>();

        public override void PopulateScore(Score score)
        {
            base.PopulateScore(score);

            score.Statistics[@"300"] = scoreResultCounts.GetOrDefault(OsuScoreResult.Hit300);
            score.Statistics[@"100"] = scoreResultCounts.GetOrDefault(OsuScoreResult.Hit100);
            score.Statistics[@"50"] = scoreResultCounts.GetOrDefault(OsuScoreResult.Hit50);
            score.Statistics[@"x"] = scoreResultCounts.GetOrDefault(OsuScoreResult.Miss);
        }

        protected override void OnNewJudgement(OsuJudgement judgement)
        {
            if (judgement != null)
            {
                if (judgement.Result != HitResult.None)
                {
                    scoreResultCounts[judgement.Score] = scoreResultCounts.GetOrDefault(judgement.Score) + 1;
                    comboResultCounts[judgement.Combo] = comboResultCounts.GetOrDefault(judgement.Combo) + 1;
                }
                switch (judgement.Score)
                {
                    case OsuScoreResult.Hit300:
                        Health.Value += (10.2 - hpDrainRate) * 0.02;
                        break;

                    case OsuScoreResult.Hit100:
                        Health.Value += (8 - hpDrainRate) * 0.02;
                        break;

                    case OsuScoreResult.Hit50:
                        Health.Value += (4 - hpDrainRate) * 0.02;
                        break;

                    case OsuScoreResult.SliderTick:
                        Health.Value += System.Math.Max(7 - hpDrainRate, 0) * 0.01;
                        break;

                    case OsuScoreResult.Miss:
                        Health.Value -= hpDrainRate * 0.04;
                        break;
                }
            }

            int score = 0;
            int maxScore = 0;

            foreach (var j in Judgements)
            {
                score += j.ScoreValue;
                maxScore += j.MaxScoreValue;
            }

            TotalScore.Value = score;
            Accuracy.Value = (double)score / maxScore;
        }
    }
}
