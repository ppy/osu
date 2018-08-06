// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// The scoring information provided by a <see cref="HitObject"/>.
    /// </summary>
    public class Judgement
    {
        /// <summary>
        /// The maximum <see cref="HitResult"/> that can be achieved.
        /// </summary>
        public virtual HitResult MaxResult => HitResult.Perfect;

        /// <summary>
        /// Whether this <see cref="Judgement"/> should affect the current combo.
        /// </summary>
        public virtual bool AffectsCombo => true;

        /// <summary>
        /// Whether this <see cref="Judgement"/> should be counted as base (combo) or bonus score.
        /// </summary>
        public virtual bool IsBonus => !AffectsCombo;

        /// <summary>
        /// The numeric score representation for the maximum achievable result.
        /// </summary>
        public int MaxNumericResult => NumericResultFor(MaxResult);

        /// <summary>
        /// Retrieves the numeric score representation of a <see cref="HitResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="HitResult"/> to find the numeric score representation for.</param>
        /// <returns>The numeric score representation of <paramref name="result"/>.</returns>
        protected virtual int NumericResultFor(HitResult result) => result > HitResult.Miss ? 1 : 0;

        /// <summary>
        /// Retrieves the numeric score representation of a <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="JudgementResult"/> to find the numeric score representation for.</param>
        /// <returns>The numeric score representation of <paramref name="result"/>.</returns>
        public int NumericResultFor(JudgementResult result) => NumericResultFor(result.Type);
    }
}
