// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinnerTick : DrawableOsuHitObject
    {
        private bool hasBonusPoints;

        /// <summary>
        /// Whether this judgement has a bonus of 1,000 points additional to the numeric result.
        /// Set when a spin occured after the spinner has completed.
        /// </summary>
        public bool HasBonusPoints
        {
            get => hasBonusPoints;
            internal set
            {
                hasBonusPoints = value;

                ((OsuSpinnerTickJudgement)Result.Judgement).HasBonusPoints = value;
                Samples.Volume.Value = value ? 1 : 0;
            }
        }

        public override bool DisplayResult => false;

        public DrawableSpinnerTick(SpinnerTick spinnerTick)
            : base(spinnerTick)
        {
        }

        /// <summary>
        /// Apply a judgement result.
        /// </summary>
        /// <param name="hit">Whether to apply a <see cref="HitResult.Great"/> result, <see cref="HitResult.Miss"/> otherwise.</param>
        internal void TriggerResult(bool hit)
        {
            HitObject.StartTime = Time.Current;
            ApplyResult(r => r.Type = hit ? HitResult.Great : HitResult.Miss);
        }
    }
}
