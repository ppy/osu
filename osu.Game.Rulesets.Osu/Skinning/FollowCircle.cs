// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
            ((DrawableSlider?)ParentObject)?.Tracking.BindValueChanged(OnTrackingChanged, true);
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
            // Gets called by slider ticks, tails, etc., leading to duplicated
            // animations which may negatively affect performance
            if (drawableObject is not DrawableSlider)
                return;

            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                OnSliderEnd();
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

        protected abstract void OnTrackingChanged(ValueChangedEvent<bool> tracking);

        protected abstract void OnSliderEnd();
    }
}
