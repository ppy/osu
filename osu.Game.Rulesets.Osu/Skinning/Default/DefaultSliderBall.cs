// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultSliderBall : CompositeDrawable
    {
        private Box box;

        private DrawableSlider slider;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, ISkinSource skin)
        {
            slider = (DrawableSlider)drawableObject;

            RelativeSizeAxes = Axes.Both;

            float radius = skin.GetConfig<OsuSkinConfiguration, float>(OsuSkinConfiguration.SliderPathRadius)?.Value ?? OsuHitObject.OBJECT_RADIUS;

            InternalChild = new CircularContainer
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(radius / OsuHitObject.OBJECT_RADIUS),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                BorderThickness = 10,
                BorderColour = Color4.White,
                Alpha = 1,
                Child = box = new Box
                {
                    Blending = BlendingParameters.Additive,
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    AlwaysPresent = true,
                    Alpha = 0
                }
            };

            slider.Tracking.BindValueChanged(trackingChanged, true);
            slider.ApplyCustomUpdateState += updateStateTransforms;
        }

        private void trackingChanged(ValueChangedEvent<bool> tracking) =>
            box.FadeTo(tracking.NewValue ? 0.3f : 0.05f, 200, Easing.OutQuint);
        
        private void updateStateTransforms(DrawableHitObject obj, ArmedState state)
        {
            using (BeginAbsoluteSequence(slider.StateUpdateTime))
            {
                this.FadeIn();
                this.ScaleTo(1f);
            }

            using (BeginAbsoluteSequence(slider.HitStateUpdateTime))
            {
                const float fade_out_time = 450;

                this.FadeOut(fade_out_time / 4, Easing.Out);
                switch (state)
                {
                    case ArmedState.Hit:
                        this.ScaleTo(1.4f, fade_out_time, Easing.Out);
                        break;
                }
            }
        }
    }
}
