// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
    internal class OsuScoreProcessor : ScoreProcessor<OsuHitObject, OsuJudgement>
    {
        public readonly Bindable<ScoringMode> Mode = new Bindable<ScoringMode>(ScoringMode.Exponential);

        public OsuScoreProcessor()
        {
        }

        public OsuScoreProcessor(RulesetContainer<OsuHitObject, OsuJudgement> rulesetContainer)
            : base(rulesetContainer)
        {
        }

        private float hpDrainRate;

        private int totalAccurateJudgements;

        private readonly Dictionary<HitResult, int> scoreResultCounts = new Dictionary<HitResult, int>();
        private readonly Dictionary<ComboResult, int> comboResultCounts = new Dictionary<ComboResult, int>();

        private double comboMaxScore;

        protected override void ComputeTargets(Beatmap<OsuHitObject> beatmap)
        {
            hpDrainRate = beatmap.BeatmapInfo.Difficulty.DrainRate;
            totalAccurateJudgements = beatmap.HitObjects.Count;

            foreach (var unused in beatmap.HitObjects)
            {
                // TODO: add support for other object types.
                AddJudgement(new OsuJudgement { Result = HitResult.Great });
            }
        }

        protected override void Reset()
        {
            base.Reset();

            Health.Value = 1;
            Accuracy.Value = 1;

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

            calculateScore();
        }

        private void calculateScore()
        {
            int baseScore = 0;
            double comboScore = 0;

            int baseMaxScore = 0;

            foreach (var j in Judgements)
            {
                baseScore += j.NumericResult;
                baseMaxScore += j.MaxNumericResult;

                comboScore += j.NumericResult * (1 + Combo.Value / 10d);
            }

            Accuracy.Value = (double)baseScore / baseMaxScore;

            if (comboScore > comboMaxScore)
                comboMaxScore = comboScore;

            if (baseScore == 0)
                TotalScore.Value = 0;
            else
            {
                // temporary to make scoring feel more like score v1 without being score v1.
                float exponentialFactor = Mode.Value == ScoringMode.Exponential ? (float)Judgements.Count / 100 : 1;

                TotalScore.Value =
                    (int)
                    (
                        exponentialFactor *
                        700000 * comboScore / comboMaxScore +
                        300000 * Math.Pow(Accuracy.Value, 10) * ((double)Judgements.Count / totalAccurateJudgements) +
                        0 /* bonusScore */
                    );
            }
        }

        public enum ScoringMode
        {
            Standardised,
            Exponential
        }
    }
}
