// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSliderTick : DrawableOsuHitObject, IRequireTracking
    {
        private const double anim_duration = 150;

        public bool Tracking { get; set; }

        public override bool DisplayJudgement => false;

        public DrawableSliderTick(SliderTick sliderTick) : base(sliderTick)
        {
            Size = new Vector2(16) * sliderTick.Scale;

            Masking = true;
            CornerRadius = Size.X / 2;

            Origin = Anchor.Centre;

            BorderThickness = 2;
            BorderColour = Color4.White;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = AccentColour,
                    Alpha = 0.3f,
                }
            };
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                AddJudgement(new OsuJudgement { Result = Tracking ? HitResult.Great : HitResult.Miss });
        }

        protected override void UpdatePreemptState()
        {
            this.Animate(
                d => d.FadeIn(anim_duration),
                d => d.ScaleTo(0.5f).ScaleTo(1.2f, anim_duration / 2)
            ).Then(
                d => d.ScaleTo(1, anim_duration / 2, Easing.Out)
            );
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut();
                    break;
                case ArmedState.Miss:
                    this.FadeOut(anim_duration)
                        .FadeColour(Color4.Red, anim_duration / 2);
                    break;
                case ArmedState.Hit:
                    this.FadeOut(anim_duration, Easing.OutQuint)
                        .ScaleTo(Scale * 1.5f, anim_duration, Easing.OutQuint);
                    break;
            }
        }
    }
}
