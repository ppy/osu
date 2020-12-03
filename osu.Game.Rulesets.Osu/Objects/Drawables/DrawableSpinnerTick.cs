// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSpinnerTick : DrawableOsuHitObject
    {
        public override bool DisplayResult => false;

        public DrawableSpinnerTick()
            : base(null)
        {
        }

        public DrawableSpinnerTick(SpinnerTick spinnerTick)
            : base(spinnerTick)
        {
        }

        private DrawableSpinner drawableSpinner;

        protected override void OnParentReceived(DrawableHitObject parent)
        {
            base.OnParentReceived(parent);
            drawableSpinner = (DrawableSpinner)parent;
        }

        protected override double MaximumJudgementOffset => drawableSpinner.HitObject.Duration;

        /// <summary>
        /// Apply a judgement result.
        /// </summary>
        /// <param name="hit">Whether this tick was reached.</param>
        internal void TriggerResult(bool hit) => ApplyResult(r => r.Type = hit ? r.Judgement.MaxResult : r.Judgement.MinResult);
    }
}
