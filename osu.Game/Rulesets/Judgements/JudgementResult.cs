// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects;
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
        /// The <see cref="HitObject"/> which was judged.
        /// </summary>
        public readonly HitObject HitObject;

        /// <summary>
        /// The <see cref="Judgement"/> which this <see cref="JudgementResult"/> applies for.
        /// </summary>
        public readonly Judgement Judgement;

        /// <summary>
        /// The time at which this <see cref="JudgementResult"/> occurred.
        /// Populated when this <see cref="JudgementResult"/> is applied via <see cref="DrawableHitObject.ApplyResult"/>.
        /// </summary>
        /// <remarks>
        /// This is used instead of <see cref="TimeAbsolute"/> to check whether this <see cref="JudgementResult"/> should be reverted.
        /// </remarks>
        internal double? RawTime { get; set; }

        /// <summary>
        /// The offset of <see cref="TimeAbsolute"/> from the end time of <see cref="HitObject"/>, clamped by <see cref="osu.Game.Rulesets.Objects.HitObject.MaximumJudgementOffset"/>.
        /// </summary>
        public double TimeOffset
        {
            get => RawTime != null ? Math.Min(RawTime.Value - HitObject.GetEndTime(), HitObject.MaximumJudgementOffset) : 0;
            internal set => RawTime = HitObject.GetEndTime() + value;
        }

        /// <summary>
        /// The absolute time at which this <see cref="JudgementResult"/> occurred, clamped by the end time of <see cref="HitObject"/> plus <see cref="osu.Game.Rulesets.Objects.HitObject.MaximumJudgementOffset"/>.
        /// </summary>
        /// <remarks>
        /// The end time of <see cref="HitObject"/> is returned if this result is not populated yet.
        /// </remarks>
        public double TimeAbsolute => RawTime != null ? Math.Min(RawTime.Value, HitObject.GetEndTime() + HitObject.MaximumJudgementOffset) : HitObject.GetEndTime();

        /// <summary>
        /// The gameplay rate at the time this <see cref="JudgementResult"/> occurred.
        /// </summary>
        public double? GameplayRate { get; internal set; }

        /// <summary>
        /// The combo prior to this <see cref="JudgementResult"/> occurring.
        /// </summary>
        public int ComboAtJudgement { get; internal set; }

        /// <summary>
        /// The combo after this <see cref="JudgementResult"/> occurred.
        /// </summary>
        public int ComboAfterJudgement { get; internal set; }

        /// <summary>
        /// The highest combo achieved prior to this <see cref="JudgementResult"/> occurring.
        /// </summary>
        public int HighestComboAtJudgement { get; internal set; }

        /// <summary>
        /// The health prior to this <see cref="JudgementResult"/> occurring.
        /// </summary>
        public double HealthAtJudgement { get; internal set; }

        /// <summary>
        /// Whether the user was in a failed state prior to this <see cref="JudgementResult"/> occurring.
        /// </summary>
        public bool FailedAtJudgement { get; internal set; }

        /// <summary>
        /// Whether a miss or hit occurred.
        /// </summary>
        public bool HasResult => Type > HitResult.None;

        /// <summary>
        /// Whether a successful hit occurred.
        /// </summary>
        public bool IsHit => Type.IsHit();

        /// <summary>
        /// The increase in health resulting from this judgement result.
        /// </summary>
        public double HealthIncrease => Judgement.HealthIncreaseFor(this);

        /// <summary>
        /// Creates a new <see cref="JudgementResult"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which was judged.</param>
        /// <param name="judgement">The <see cref="Judgement"/> to refer to for scoring information.</param>
        public JudgementResult(HitObject hitObject, Judgement judgement)
        {
            HitObject = hitObject;
            Judgement = judgement;
            Reset();
        }

        internal void Reset()
        {
            Type = HitResult.None;
            RawTime = null;
        }

        public override string ToString() => $"{Type} ({Judgement})";
    }
}
