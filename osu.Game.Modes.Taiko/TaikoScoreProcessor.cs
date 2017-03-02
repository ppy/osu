using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;

namespace osu.Game.Modes.Taiko
{
    class TaikoScoreProcessor : ScoreProcessor
    {
        public TaikoScoreProcessor(int hitObjectCount)
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
                        //Health.Value -= 0.2f;
                        break;
                }
            }

            int score = 0;
            int maxScore = 0;

            foreach (TaikoJudgementInfo j in Judgements)
            {
                score += j.ScoreValue;
                maxScore += j.MaxScoreValue;
            }

            TotalScore.Value = score;
            Accuracy.Value = (double)score / maxScore;
        }
    }
}
