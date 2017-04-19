// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
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

        public OsuScoreProcessor(HitRenderer<OsuHitObject, OsuJudgement> hitRenderer)
            : base(hitRenderer)
        {
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

        public override Score CreateEmptyScore() => new OsuScore();

        public override Score GetPopulatedScore()
        {
            var score = (OsuScore)base.GetPopulatedScore();

            scoreResultCounts.TryGetValue(OsuScoreResult.Hit300, out score.Count300);
            scoreResultCounts.TryGetValue(OsuScoreResult.Hit100, out score.Count100);
            scoreResultCounts.TryGetValue(OsuScoreResult.Hit50, out score.Count50);
            scoreResultCounts.TryGetValue(OsuScoreResult.Miss, out score.CountMiss);

            return score;
        }

        protected override void OnNewJudgement(OsuJudgement judgement)
        {
            if (judgement != null)
            {
                if (judgement.Result != HitResult.None)
                {
                    int count;
                    if (scoreResultCounts.TryGetValue(judgement.Score, out count))
                        scoreResultCounts[judgement.Score] = count + 1;
                    else
                        scoreResultCounts[judgement.Score] = 0;

                    if (comboResultCounts.TryGetValue(judgement.Combo, out count))
                        comboResultCounts[judgement.Combo] = count + 1;
                    else
                        comboResultCounts[judgement.Combo] = 0;
                }

                switch (judgement.Result)
                {
                    case HitResult.Hit:
                        Health.Value += 0.1f;
                        break;
                    case HitResult.Miss:
                        Health.Value -= 0.2f;
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
