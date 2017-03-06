// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        public int AccuracyScoreValue => AccuracyScoreToInt(Score);

        public int MaxScoreValue => ScoreToInt(MaxScore);
        public int MaxAccuracyScoreValue => AccuracyScoreToInt(MaxScore);

        public bool SecondHit;

        /// <summary>
        /// This is used to compute score for the score processor.
        /// </summary>
        protected virtual int ScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoScoreResult.Good:
                    return 100;
                case TaikoScoreResult.Great:
                    return 300;
            }
        }

        /// <summary>
        /// This is used to compute the score for the accuracy percentage.
        /// </summary>
        protected virtual int AccuracyScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoScoreResult.Great:
                    return 300;
                case TaikoScoreResult.Good:
                    return 150;
            }
        }
    }
}
