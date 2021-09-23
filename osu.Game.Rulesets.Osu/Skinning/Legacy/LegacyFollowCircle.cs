// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacyFollowCircle : CompositeDrawable
    {
        private readonly Drawable animationContent;
        private DrawableSlider slider;

        public LegacyFollowCircle(Drawable animationContent)
        {
            this.animationContent = animationContent;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject)
        {
            slider = (DrawableSlider)drawableObject;

            RelativeSizeAxes = Axes.Both;

            InternalChild = animationContent;
            animationContent.Anchor = Anchor.Centre;
            animationContent.Origin = Anchor.Centre;

            slider.Tracking.BindValueChanged(trackingChanged, true);
        }

        private void trackingChanged(ValueChangedEvent<bool> e)
        {
            bool tracking = e.NewValue;

            if (slider.Ball.InputTracksVisualSize)
            {
                if (tracking)
                    this.ScaleTo(DrawableSliderBall.FOLLOW_AREA, 200, Easing.OutQuint);
                else
                    this.ScaleTo(1.9f, 200, Easing.None);
            }
            else
            {
                // We need to always be tracking the final size, at both endpoints. For now, this is achieved by removing the scale duration.
                this.ScaleTo(tracking ? DrawableSliderBall.FOLLOW_AREA : 1f);
            }

            if (tracking)
                this.FadeIn(100, Easing.OutQuint);
            else
                this.FadeOut(200, Easing.InQuint);
        }
    }
}
