// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Judgements
{
    public class Judgement
    {
        /// <summary>
        /// Whether this judgement is the result of a hit or a miss.
        /// </summary>
        public HitResult Result;

        /// <summary>
        /// The maximum <see cref="HitResult"/> that can be achieved.
        /// </summary>
        public virtual HitResult MaxResult => HitResult.Perfect;

        public bool IsHit => Result > HitResult.Miss;

        /// <summary>
        /// The offset from a perfect hit at which this judgement occurred.
        /// Populated when added via <see cref="DrawableHitObject{TObject}.AddJudgement"/>.
        /// </summary>
        public double TimeOffset { get; internal set; }

        /// <summary>
        /// Whether the <see cref="Result"/> should affect the combo portion of the score.
        /// If false, the <see cref="Result"/> will be considered for the bonus portion of the score.
        /// </summary>
        public virtual bool AffectsCombo => true;

        /// <summary>
        /// The numeric representation for the result achieved.
        /// </summary>
        public int NumericResult => NumericResultFor(Result);

        /// <summary>
        /// The numeric representation for the maximum achievable result.
        /// </summary>
        public int MaxNumericResult => NumericResultFor(MaxResult);

        /// <summary>
        /// Convert a <see cref="HitResult"/> to a numeric score representation.
        /// </summary>
        /// <param name="result">The value to convert.</param>
        /// <returns>The number.</returns>
        protected virtual int NumericResultFor(HitResult result) => result > HitResult.Miss ? 1 : 0;
    }
}
