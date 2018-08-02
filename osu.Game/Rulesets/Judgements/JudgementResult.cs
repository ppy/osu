// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
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
        /// Populated when added via <see cref="DrawableHitObject.ApplyJudgement"/>.
        /// </summary>
        public double TimeOffset { get; internal set; }

        /// <summary>
        /// The combo prior to this judgement occurring.
        /// </summary>
        public int ComboAtJudgement { get; internal set; }

        /// <summary>
        /// The highest combo achieved prior to this judgement occurring.
        /// </summary>
        public int HighestComboAtJudgement { get; internal set; }

        /// <summary>
        /// Whether this <see cref="Judgement"/> has a result.
        /// </summary>
        public bool HasResult => Type > HitResult.None;

        /// <summary>
        /// Whether a successful hit occurred.
        /// </summary>
        public bool IsHit => Type > HitResult.Miss;

        public JudgementResult(Judgement judgement)
        {
            Judgement = judgement;
        }
    }
}
