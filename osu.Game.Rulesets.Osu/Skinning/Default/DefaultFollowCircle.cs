// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultFollowCircle : CompositeDrawable
    {
        private DrawableSlider slider;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            slider = (DrawableSlider)drawableObject;

            RelativeSizeAxes = Axes.Both;
            Alpha = 1f;

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

            slider.Tracking.BindValueChanged(trackingChanged, true);
            slider.HitObjectApplied += hitObjectApplied;
        }

        private void trackingChanged(ValueChangedEvent<bool> e)
        {
            bool tracking = e.NewValue;

            if (slider.Ball.InputTracksVisualSize)
                this.ScaleTo(tracking ? DrawableSliderBall.FOLLOW_AREA: 1f, 300, Easing.OutQuint);
            else
            {
                // We need to always be tracking the final size, at both endpoints. For now, this is achieved by removing the scale duration.
                this.ScaleTo(tracking ? DrawableSliderBall.FOLLOW_AREA : 1f);
            }

            this.FadeTo(tracking ? 1f : 0, 300, Easing.OutQuint);
        }

        private void hitObjectApplied(DrawableHitObject obj)
        {
            using (BeginAbsoluteSequence(slider.HitObject.EndTime))
            {
                const float fade_out_time = 450;

                this.FadeOut(fade_out_time / 4, Easing.Out);
            }
        }
    }
}
