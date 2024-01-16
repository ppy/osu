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
    public class Judgement
    {
        /// <summary>
        /// Whether this <see cref="Judgement"/> is the result of a hit or a miss.
        /// </summary>
        public HitResult Type;

        /// <summary>
        /// The <see cref="HitObject"/> which was judged.
        /// </summary>
        public readonly HitObject HitObject;

        /// <summary>
        /// The <see cref="JudgementCriteria"/> which this <see cref="Judgement"/> applies for.
        /// </summary>
        public readonly JudgementCriteria JudgementCriteria;

        /// <summary>
        /// The time at which this <see cref="Judgement"/> occurred.
        /// Populated when this <see cref="Judgement"/> is applied via <see cref="DrawableHitObject.ApplyResult"/>.
        /// </summary>
        /// <remarks>
        /// This is used instead of <see cref="TimeAbsolute"/> to check whether this <see cref="Judgement"/> should be reverted.
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
        /// The absolute time at which this <see cref="Judgement"/> occurred, clamped by the end time of <see cref="HitObject"/> plus <see cref="osu.Game.Rulesets.Objects.HitObject.MaximumJudgementOffset"/>.
        /// </summary>
        /// <remarks>
        /// The end time of <see cref="HitObject"/> is returned if this result is not populated yet.
        /// </remarks>
        public double TimeAbsolute => RawTime != null ? Math.Min(RawTime.Value, HitObject.GetEndTime() + HitObject.MaximumJudgementOffset) : HitObject.GetEndTime();

        /// <summary>
        /// The gameplay rate at the time this <see cref="Judgement"/> occurred.
        /// </summary>
        public double? GameplayRate { get; internal set; }

        /// <summary>
        /// The combo prior to this <see cref="Judgement"/> occurring.
        /// </summary>
        public int ComboAtJudgement { get; internal set; }

        /// <summary>
        /// The combo after this <see cref="Judgement"/> occurred.
        /// </summary>
        public int ComboAfterJudgement { get; internal set; }

        /// <summary>
        /// The highest combo achieved prior to this <see cref="Judgement"/> occurring.
        /// </summary>
        public int HighestComboAtJudgement { get; internal set; }

        /// <summary>
        /// The health prior to this <see cref="Judgement"/> occurring.
        /// </summary>
        public double HealthAtJudgement { get; internal set; }

        /// <summary>
        /// Whether the user was in a failed state prior to this <see cref="Judgement"/> occurring.
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
        /// Creates a new <see cref="Judgement"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> which was judged.</param>
        /// <param name="judgementCriteria">The <see cref="JudgementCriteria"/> to refer to for scoring information.</param>
        public Judgement(HitObject hitObject, JudgementCriteria judgementCriteria)
        {
            HitObject = hitObject;
            JudgementCriteria = judgementCriteria;
            Reset();
        }

        internal void Reset()
        {
            Type = HitResult.None;
            RawTime = null;
        }

        public override string ToString() => $"{Type} ({JudgementCriteria})";
    }
}
