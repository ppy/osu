//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
