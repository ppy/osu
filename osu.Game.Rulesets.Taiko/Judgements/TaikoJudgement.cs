// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoJudgement : Judgement
    {
        /// <summary>
        /// The result value for the accuracy portion of the score.
        /// </summary>
        public int ResultNumericForAccuracy => Result == HitResult.Miss ? 0 : NumericResultForAccuracy(Result);

        /// <summary>
        /// The maximum result value for the accuracy portion of the score.
        /// </summary>
        public int MaxResultValueForAccuracy => NumericResultForAccuracy(HitResult.Great);

        /// <summary>
        /// Whether this Judgement has a secondary hit in the case of strong hits.
        /// </summary>
        public virtual bool SecondHit { get; set; }

        /// <summary>
        /// Computes the numeric result value for the combo portion of the score.
        /// For the accuracy portion of the score (including accuracy percentage), see <see cref="NumericResultForAccuracy(HitResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the value for.</param>
        /// <returns>The numeric result value.</returns>
        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Good:
                    return 100;
                case HitResult.Great:
                    return 300;
            }
        }

        /// <summary>
        /// Computes the numeric result value for the accuracy portion of the score.
        /// For the combo portion of the score, see <see cref="NumericResultFor(HitResult)"/>.
        /// </summary>
        /// <param name="result">The result to compute the value for.</param>
        /// <returns>The numeric result value.</returns>
        protected virtual int NumericResultForAccuracy(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Good:
                    return 150;
                case HitResult.Great:
                    return 300;
            }
        }
    }
}
