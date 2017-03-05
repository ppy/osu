using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects
{
    public class TaikoJudgementInfo : JudgementInfo
    {
        /// <summary>
        /// The score the user achieved.
        /// </summary>
        public TaikoScoreResult Score;

        /// <summary>
        /// The score which would be achievable on a perfect hit.
        /// </summary>
        public TaikoScoreResult MaxScore = TaikoScoreResult.Great;

        public int ScoreValue => ScoreToInt(Score);

        public int MaxScoreValue => ScoreToInt(MaxScore);

        public bool SecondHit;

        protected virtual int ScoreToInt(TaikoScoreResult result)
        {
            int score = 0;

            switch (result)
            {
                default:
                case TaikoScoreResult.Good:
                    score = 100;
                    break;
                case TaikoScoreResult.Great:
                    score = 300;
                    break;
            }

            return score;
        }
    }
}
