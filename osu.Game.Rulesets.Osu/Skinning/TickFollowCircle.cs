// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract class TickFollowCircle : FollowCircle
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ParentObject != null)
                ParentObject.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            // Fine to use drawableObject.HitStateUpdateTime even for DrawableSliderTail, since on
            // stable, the break anim plays right when the tail is missed, not when the slider ends
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        if (drawableObject is DrawableSliderTick or DrawableSliderRepeat)
                            OnSliderTick();
                        break;

                    case ArmedState.Miss:
                        if (drawableObject is DrawableSliderTail or DrawableSliderTick or DrawableSliderRepeat)
                            OnSliderBreak();
                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (ParentObject != null)
                ParentObject.ApplyCustomUpdateState -= updateStateTransforms;
        }

        /// <summary>
        /// Sealed empty intentionally. Override <see cref="OnSliderBreak" /> instead, since
        /// animations should only play on slider ticks.
        /// </summary>
        protected sealed override void OnSliderRelease()
        {
        }

        protected abstract void OnSliderTick();

        protected abstract void OnSliderBreak();
    }
}
