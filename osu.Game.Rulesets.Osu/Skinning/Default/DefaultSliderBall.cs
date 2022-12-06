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
    public partial class DefaultSliderBall : CompositeDrawable
    {
        private Box box = null!;

        [Resolved(canBeNull: true)]
        private DrawableHitObject? parentObject { get; set; }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
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

        private void trackingChanged(ValueChangedEvent<bool> tracking) =>
            box.FadeTo(tracking.NewValue ? 0.3f : 0.05f, 200, Easing.OutQuint);

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState state)
        {
            // Gets called by slider ticks, tails, etc., leading to duplicated
            // animations which may negatively affect performance
            if (drawableObject is not DrawableSlider)
                return;

            const float fade_duration = 450f;

            using (BeginAbsoluteSequence(drawableObject.StateUpdateTime))
            {
                this.FadeIn()
                    .ScaleTo(1f);
            }

            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
            {
                // intentionally pile on an extra FadeOut to make it happen much faster
                this.FadeOut(fade_duration / 4, Easing.Out);

                switch (state)
                {
                    case ArmedState.Hit:
                        this.ScaleTo(1.4f, fade_duration, Easing.Out);
                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (parentObject != null)
                parentObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
