// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// The scoring result of a <see cref="DrawableHitObject"/>.
    /// </summary>
    public class JudgementResult
    {
        /// <summary>
        /// Whether this <see cref="JudgementResult"/> is the result of a hit or a miss.
        /// </summary>
        public HitResult Type;

        /// <summary>
        /// The <see cref="Judgement"/> which this <see cref="JudgementResult"/> applies for.
        /// </summary>
        public readonly Judgement Judgement;

        /// <summary>
        /// The offset from a perfect hit at which this <see cref="JudgementResult"/> occurred.
        /// Populated when this <see cref="JudgementResult"/> is applied via <see cref="DrawableHitObject.ApplyResult"/>.
        /// </summary>
        public double TimeOffset { get; internal set; }

        /// <summary>
        /// The combo prior to this <see cref="JudgementResult"/> occurring.
        /// </summary>
        public int ComboAtJudgement { get; internal set; }

        /// <summary>
        /// The highest combo achieved prior to this <see cref="JudgementResult"/> occurring.
        /// </summary>
        public int HighestComboAtJudgement { get; internal set; }

        /// <summary>
        /// Whether a miss or hit occurred.
        /// </summary>
        public bool HasResult => Type > HitResult.None;

        /// <summary>
        /// Whether a successful hit occurred.
        /// </summary>
        public bool IsHit => Type > HitResult.Miss;

        /// <summary>
        /// Creates a new <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="judgement">The <see cref="Judgement"/> to refer to for scoring information.</param>
        public JudgementResult(Judgement judgement)
        {
            Judgement = judgement;
        }

        public override string ToString() => $"{Type} (Score:{Judgement.NumericResultFor(this)} HP:{Judgement.HealthIncreaseFor(this)} {Judgement})";
    }
}
