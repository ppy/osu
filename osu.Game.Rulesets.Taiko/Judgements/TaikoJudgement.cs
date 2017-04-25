// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoJudgement : Judgement
    {
        /// <summary>
        /// The maximum result.
        /// </summary>
        public const TaikoHitResult MAX_HIT_RESULT = TaikoHitResult.Great;

        /// <summary>
        /// The result.
        /// </summary>
        public TaikoHitResult TaikoResult;

        /// <summary>
        /// The result value for the combo portion of the score.
        /// </summary>
        public int ResultValueForScore => Result == HitResult.Miss ? 0 : NumericResultForScore(TaikoResult);

        /// <summary>
        /// The result value for the accuracy portion of the score.
        /// </summary>
        public int ResultValueForAccuracy => Result == HitResult.Miss ? 0 : NumericResultForAccuracy(TaikoResult);

        /// <summary>
        /// The maximum result value for the combo portion of the score.
        /// </summary>
        public int MaxResultValueForScore => NumericResultForScore(MAX_HIT_RESULT);

        /// <summary>
        /// The maximum result value for the accuracy portion of the score.
        /// </summary>
        public int MaxResultValueForAccuracy => NumericResultForAccuracy(MAX_HIT_RESULT);

        public override string ResultString => TaikoResult.GetDescription();

        public override string MaxResultString => MAX_HIT_RESULT.GetDescription();

        /// <summary>
        /// Whether this Judgement has a secondary hit in the case of strong hits.
        /// </summary>
        public virtual bool SecondHit { get; set; }

        /// <summary>
        /// Computes the numeric result value for the combo portion of the score.
        /// For the accuracy portion of the score (including accuracy percentage), see <see cref="NumericResultForAccuracy(TaikoHitResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the value for.</param>
        /// <returns>The numeric result value.</returns>
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
        /// Computes the numeric result value for the accuracy portion of the score.
        /// For the combo portion of the score, see <see cref="NumericResultForScore(TaikoHitResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the value for.</param>
        /// <returns>The numeric result value.</returns>
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
