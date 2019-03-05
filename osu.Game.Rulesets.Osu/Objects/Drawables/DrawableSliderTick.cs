// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderTick : DrawableOsuHitObject, IRequireTracking
    {
        public const double ANIM_DURATION = 150;

        public bool Tracking { get; set; }

        public override bool DisplayResult => false;

        public DrawableSliderTick(SliderTick sliderTick)
            : base(sliderTick)
        {
            Size = new Vector2(16) * sliderTick.Scale;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                new SkinnableDrawable("Play/osu/sliderscorepoint", _ => new Container
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    CornerRadius = Size.X / 2,

                    BorderThickness = 2,
                    BorderColour = Color4.White,

                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = AccentColour,
                        Alpha = 0.3f,
                    }
                }, restrictSize: false)
            };
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                ApplyResult(r => r.Type = Tracking ? HitResult.Great : HitResult.Miss);
        }

        protected override void UpdatePreemptState()
        {
            this.FadeOut().FadeIn(ANIM_DURATION);
            this.ScaleTo(0.5f).ScaleTo(1f, ANIM_DURATION * 4, Easing.OutElasticHalf);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut();
                    break;
                case ArmedState.Miss:
                    this.FadeOut(ANIM_DURATION);
                    this.FadeColour(Color4.Red, ANIM_DURATION / 2);
                    break;
                case ArmedState.Hit:
                    this.FadeOut(ANIM_DURATION, Easing.OutQuint);
                    this.ScaleTo(Scale * 1.5f, ANIM_DURATION, Easing.Out);
                    break;
            }
        }
    }
}
