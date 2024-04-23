// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Skinning;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableSliderTick : DrawableOsuHitObject
    {
        public const double ANIM_DURATION = 150;

        public const float DEFAULT_TICK_SIZE = 16;

        public DrawableSlider DrawableSlider => (DrawableSlider)ParentHitObject;

        private SkinnableDrawable scaleContainer;

        public DrawableSliderTick()
            : base(null)
        {
        }

        public DrawableSliderTick(SliderTick sliderTick)
            : base(sliderTick)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = OsuHitObject.OBJECT_DIMENSIONS;
            Origin = Anchor.Centre;

            AddInternal(scaleContainer = new SkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.SliderScorePoint), _ => new CircularContainer
            {
                Masking = true,
                Origin = Anchor.Centre,
                Size = new Vector2(DEFAULT_TICK_SIZE),
                BorderThickness = DEFAULT_TICK_SIZE / 4,
                BorderColour = Color4.White,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = AccentColour.Value,
                    Alpha = 0.3f,
                }
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            ScaleBindable.BindValueChanged(scale => scaleContainer.Scale = new Vector2(scale.NewValue));
        }

        protected override void OnApply()
        {
            base.OnApply();

            Position = HitObject.Position - DrawableSlider.HitObject.Position;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset) => DrawableSlider.SliderInputManager.TryJudgeNestedObject(this, timeOffset);

        protected override void UpdateInitialTransforms()
        {
            this.FadeOut().FadeIn(ANIM_DURATION);
            this.ScaleTo(0.5f).ScaleTo(1f, ANIM_DURATION * 4, Easing.OutElasticHalf);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(ANIM_DURATION, Easing.OutQuint);
                    break;

                case ArmedState.Hit:
                    this.FadeOut(ANIM_DURATION, Easing.OutQuint);
                    this.ScaleTo(Scale * 1.5f, ANIM_DURATION, Easing.Out);
                    break;
            }
        }
    }
}
