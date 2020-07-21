// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinnerTick : DrawableOsuHitObject
    {
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
