// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSpinnerTick : DrawableOsuHitObject
    {
        public override bool DisplayResult => false;

        public DrawableSpinnerTick()
            : this(null)
        {
        }

        public DrawableSpinnerTick(SpinnerTick spinnerTick)
            : base(spinnerTick)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        protected override void OnApply()
        {
            base.OnApply();

            // Lifetime will be managed by `DrawableSpinner`.
            LifetimeStart = double.MaxValue;
        }

        /// <summary>
        /// Apply a judgement result.
        /// </summary>
        /// <param name="hit">Whether this tick was reached.</param>
        internal void TriggerResult(bool hit)
        {
            if (hit)
                ApplyMaxResult();
            else
                ApplyMinResult();
        }
    }
}
