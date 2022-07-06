// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultFollowCircle : CompositeDrawable
    {
        [Resolved(canBeNull: true)]
        private DrawableHitObject? parentObject { get; set; }

        public DefaultFollowCircle()
        {
            Alpha = 0f;
            RelativeSizeAxes = Axes.Both;

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 5,
                BorderColour = Color4.Orange,
                Blending = BlendingParameters.Additive,
                Child = new Box
                {
                    Colour = Color4.Orange,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (parentObject != null)
            {
                var slider = (DrawableSlider)parentObject;
                slider.Tracking.BindValueChanged(trackingChanged, true);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (parentObject != null)
            {
                parentObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(parentObject, parentObject.State.Value);
            }
        }

        private void trackingChanged(ValueChangedEvent<bool> tracking)
        {
            const float scale_duration = 300f;
            const float fade_duration = 300f;

            this.ScaleTo(tracking.NewValue ? DrawableSliderBall.FOLLOW_AREA : 1f, scale_duration, Easing.OutQuint)
                .FadeTo(tracking.NewValue ? 1f : 0, fade_duration, Easing.OutQuint);
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            // see comment in LegacySliderBall.updateStateTransforms
            if (drawableObject is not DrawableSlider)
                return;

            const float fade_duration = 450f;

            // intentionally pile on an extra FadeOut to make it happen much faster
            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                this.FadeOut(fade_duration / 4, Easing.Out);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (parentObject != null)
                parentObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
