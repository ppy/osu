// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

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

        /// <summary>
        /// The combo prior to this judgement occurring.
        /// </summary>
        public int ComboAtJudgement;

        /// <summary>
        /// The highest combo achieved prior to this judgement occurring.
        /// </summary>
        public int HighestComboAtJudgement;

        /// <summary>
        /// Whether a successful hit occurred.
        /// </summary>
        public bool IsHit => Result > HitResult.Miss;

        /// <summary>
        /// Whether this judgement is the final judgement for the hit object.
        /// </summary>
        public bool Final = true;

        /// <summary>
        /// The offset from a perfect hit at which this judgement occurred.
        /// Populated when added via <see cref="DrawableHitObject{TObject}.AddJudgement"/>.
        /// </summary>
        public double TimeOffset { get; set; }

        /// <summary>
        /// Whether the <see cref="Result"/> should affect the current combo.
        /// </summary>
        public virtual bool AffectsCombo => true;

        /// <summary>
        /// Whether the <see cref="Result"/> should be counted as base (combo) or bonus score.
        /// </summary>
        public virtual bool IsBonus => !AffectsCombo;

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
