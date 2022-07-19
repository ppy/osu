// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public abstract class FollowCircle : CompositeDrawable
    {
        [Resolved]
        protected DrawableHitObject? ParentObject { get; private set; }

        protected FollowCircle()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ((DrawableSlider?)ParentObject)?.Tracking.BindValueChanged(tracking =>
            {
                Debug.Assert(ParentObject != null);
                if (ParentObject.Judged)
                    return;

                if (tracking.NewValue)
                    OnSliderPress();
                else
                    OnSliderRelease();
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (ParentObject != null)
            {
                ParentObject.HitObjectApplied += onHitObjectApplied;
                onHitObjectApplied(ParentObject);

                ParentObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(ParentObject, ParentObject.State.Value);
            }
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            this.ScaleTo(1f)
                .FadeOut();
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            // We only want DrawableSlider here. DrawableSliderTail doesn't quite work because its
            // HitStateUpdateTime is ~36ms before DrawableSlider's HitStateUpdateTime (aka slider
            // end leniency).
            if (drawableObject is not DrawableSlider)
                return;

            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        OnSliderTail();
                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (ParentObject != null)
            {
                ParentObject.HitObjectApplied -= onHitObjectApplied;
                ParentObject.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }

        protected abstract void OnSliderPress();

        protected abstract void OnSliderRelease();

        protected abstract void OnSliderTail();
    }
}
