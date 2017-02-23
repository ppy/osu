using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects
{
    /// <summary>
    /// A "double judgement" of sorts.
    /// </summary>
    public class TaikoFinisherJudgementInfo : TaikoJudgementInfo
    {
        public TaikoJudgementInfo FirstHitJudgement = new TaikoJudgementInfo();

        public override int ScoreValue => ScoreToInt(Score) + FirstHitJudgement.ScoreValue;

        public override int MaxScoreValue => ScoreToInt(MaxScore) + FirstHitJudgement.MaxScoreValue;

        protected override int ScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                case TaikoScoreResult.Miss:
                    return 0;
                case TaikoScoreResult.Good:
                    return 300;
                case TaikoScoreResult.Great:
                    return 900;
            }
        }
    }
}
