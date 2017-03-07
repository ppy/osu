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

        /// <summary>
        /// Returns the score value of this judgement.
        /// </summary>
        public int ScoreValue => ScoreToInt(Score);

        /// <summary>
        /// Returns the accuracy score value of this judgement.
        /// </summary>
        public int AccuracyScoreValue => AccuracyScoreToInt(Score);
        
        /// <summary>
        /// Returns the maximum score value of this judgement.
        /// </summary>
        public int MaxScoreValue => ScoreToInt(MaxScore);

        /// <summary>
        /// Returns the maximum accuracy score value of this judgement.
        /// </summary>
        public int MaxAccuracyScoreValue => AccuracyScoreToInt(MaxScore);

        public bool SecondHit;

        /// <summary>
        /// Converts the score value to a raw score.
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
        /// Converts the accuracy score value to a raw accuracy score.
        /// <para>This is used specifically to compute the accuracy percentage.</para>
        /// </summary>
        protected virtual int AccuracyScoreToInt(TaikoScoreResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoScoreResult.Good:
                    return 150;
                case TaikoScoreResult.Great:
                    return 300;
            }
        }
    }
}
