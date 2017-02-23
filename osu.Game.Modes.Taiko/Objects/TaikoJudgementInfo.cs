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

        public virtual int ScoreValue => ScoreToInt(Score);

        public virtual int MaxScoreValue => ScoreToInt(MaxScore);

        protected virtual int ScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                case TaikoScoreResult.Miss:
                    return 0;
                case TaikoScoreResult.Good:
                    return 100;
                case TaikoScoreResult.Great:
                    return 300;
            }
        }
    }
}
