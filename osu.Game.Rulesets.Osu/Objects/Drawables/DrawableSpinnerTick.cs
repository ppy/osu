// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSpinnerTick : DrawableOsuHitObject
    {
        public override bool DisplayResult => false;

        protected DrawableSpinner DrawableSpinner => (DrawableSpinner)ParentHitObject;

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

            // the tick can be theoretically judged at any point in the spinner's duration,
            // so it must be alive throughout the spinner's entire lifetime.
            // this mostly matters for correct sample playback.
            LifetimeStart = DrawableSpinner.HitObject.StartTime;
        }

        /// <summary>
        /// Apply a judgement result.
        /// </summary>
        /// <param name="hit">Whether this tick was reached.</param>
        internal void TriggerResult(bool hit) => ApplyResult(r => r.Type = hit ? r.Judgement.MaxResult : r.Judgement.MinResult);
    }
}
