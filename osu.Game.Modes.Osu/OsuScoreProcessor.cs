// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables;

namespace osu.Game.Modes.Osu
{
    class OsuScoreProcessor : ScoreProcessor
    {
        public OsuScoreProcessor(int hitObjectCount)
            : base(hitObjectCount)
        {
            Health.Value = 1;
        }

        protected override void UpdateCalculations(JudgementInfo judgement)
        {
            if (judgement != null)
            {
                switch (judgement.Result)
                {
                    case HitResult.Hit:
                        Combo.Value++;
                        Health.Value += 0.1f;
                        break;
                    case HitResult.Miss:
                        Combo.Value = 0;
                        Health.Value -= 0.2f;
                        break;
                }
            }

            int score = 0;
            int maxScore = 0;

            foreach (OsuJudgementInfo j in Judgements)
            {
                score += j.ScoreValue;
                maxScore += j.MaxScoreValue;
            }

            TotalScore.Value = score;
            Accuracy.Value = (double)score / maxScore;
        }
    }
}
