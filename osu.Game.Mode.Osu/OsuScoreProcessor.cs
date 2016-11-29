using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables;

namespace osu.Game.Modes.Osu
{
    class OsuScoreProcessor : ScoreProcessor
    {
        public override void AddJudgement(JudgementInfo judgement)
        {
            base.AddJudgement(judgement);

            switch (judgement.Result)
            {
                case HitResult.Hit:
                    Combo.Value++;
                    break;
                case HitResult.Miss:
                    Combo.Value = 0;
                    break;
            }
        }
        protected override void UpdateCalculations()
        {
            base.UpdateCalculations();

            int score = 0;
            int maxScore = 0;

            foreach (OsuJudgementInfo j in Judgements)
            {
                switch (j.Score)
                {
                    case OsuScoreResult.Miss:
                        maxScore += 300;
                        break;
                    case OsuScoreResult.Hit50:
                        score += 50;
                        maxScore += 300;
                        break;
                    case OsuScoreResult.Hit100:
                        score += 100;
                        maxScore += 300;
                        break;
                    case OsuScoreResult.Hit300:
                        score += 300;
                        maxScore += 300;
                        break;
                }

                
            }

            TotalScore.Value = score;
            Accuracy.Value = (double)score / maxScore;
        }
    }
}
