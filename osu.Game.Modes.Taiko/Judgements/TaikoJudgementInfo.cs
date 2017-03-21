// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Judgements;

namespace osu.Game.Modes.Taiko.Judgements
{
    public class TaikoJudgementInfo : JudgementInfo
    {
        /// <summary>
        /// The maximum score value.
        /// </summary>
        public const TaikoScoreResult MAX_SCORE = TaikoScoreResult.Great;

        /// <summary>
        /// The score value.
        /// </summary>
        public TaikoScoreResult Score;

        /// <summary>
        /// The score value for the combo portion of the score.
        /// </summary>
        public int ScoreValue => ScoreToInt(Score);
        
        /// <summary>
        /// The score value for the accuracy portion of the score.
        /// </summary>
        public int AccuracyScoreValue => AccuracyScoreToInt(Score);

        /// <summary>
        /// The maximum score value for the combo portion of the score.
        /// </summary>
        public int MaxScoreValue => ScoreToInt(MAX_SCORE);
        
        /// <summary>
        /// The maximum score value for the accuracy portion of the score.
        /// </summary>
        public int MaxAccuracyScoreValue => AccuracyScoreToInt(MAX_SCORE);

        /// <summary>
        /// Whether this Judgement has a secondary hit in the case of finishers.
        /// </summary>
        public bool SecondHit;

        /// <summary>
        /// Computes the score value for the combo portion of the score.
        /// For the accuracy portion of the score (including accuracy percentage), see <see cref="AccuracyScoreToInt(TaikoScoreResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the score value for.</param>
        /// <returns>The int score value.</returns>
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
        /// Computes the score value for the accuracy portion of the score.
        /// </summary>
        /// <param name="result">The result to compute the score value for.</param>
        /// <returns>The int score value.</returns>
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
