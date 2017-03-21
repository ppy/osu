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
        public const TaikoHitResult MAX_HIT_RESULT = TaikoHitResult.Great;

        /// <summary>
        /// The score value.
        /// </summary>
        public TaikoHitResult TaikoResult;

        /// <summary>
        /// The score value for the combo portion of the score.
        /// </summary>
        public int ScoreValue => NumericResultForScore(TaikoResult);
        
        /// <summary>
        /// The score value for the accuracy portion of the score.
        /// </summary>
        public int AccuracyScoreValue => NumericResultForAccuracy(TaikoResult);

        /// <summary>
        /// The maximum score value for the combo portion of the score.
        /// </summary>
        public int MaxScoreValue => NumericResultForScore(MAX_HIT_RESULT);
        
        /// <summary>
        /// The maximum score value for the accuracy portion of the score.
        /// </summary>
        public int MaxAccuracyScoreValue => NumericResultForAccuracy(MAX_HIT_RESULT);

        /// <summary>
        /// Whether this Judgement has a secondary hit in the case of finishers.
        /// </summary>
        public bool SecondHit;

        /// <summary>
        /// Computes the numeric score value for the combo portion of the score.
        /// For the accuracy portion of the score (including accuracy percentage), see <see cref="NumericResultForAccuracy(TaikoHitResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the score value for.</param>
        /// <returns>The numeric score value.</returns>
        protected virtual int NumericResultForScore(TaikoHitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoHitResult.Good:
                    return 100;
                case TaikoHitResult.Great:
                    return 300;
            }
        }

        /// <summary>
        /// Computes the numeric score value for the accuracy portion of the score.
        /// For the combo portion of the score, see <see cref="NumericResultForScore(TaikoHitResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the score value for.</param>
        /// <returns>The numeric score value.</returns>
        protected virtual int NumericResultForAccuracy(TaikoHitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case TaikoHitResult.Good:
                    return 150;
                case TaikoHitResult.Great:
                    return 300;
            }
        }
    }
}
