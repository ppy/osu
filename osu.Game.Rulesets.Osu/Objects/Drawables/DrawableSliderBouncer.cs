// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Classic.Objects.Drawables
{
    public class DrawableSliderBouncer : DrawableOsuHitObject
    {
        private readonly SliderBouncer sliderBouncer;
        private readonly DrawableSlider drawableSlider;

        public double FadeInTime;
        public double FadeOutTime;

        public override bool RemoveWhenNotAlive => false;

        public DrawableSliderBouncer(SliderBouncer sliderBouncer, DrawableSlider drawableSlider) : base(sliderBouncer)
        {
            this.sliderBouncer = sliderBouncer;
            this.drawableSlider = drawableSlider;

            AutoSizeAxes = Axes.Both;
            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Icon = FontAwesome.fa_eercast,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(32),
                }
            };
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (sliderBouncer.StartTime <= Time.Current)
                AddJudgement(new OsuJudgement { Result = drawableSlider.Tracking ? HitResult.Great : HitResult.Miss });
        }

        protected override void UpdatePreemptState()
        {
            var animIn = Math.Min(150, sliderBouncer.StartTime - FadeInTime);

            this.Animate(
                d => d.FadeIn(animIn),
                d => d.ScaleTo(0.5f).ScaleTo(1.2f, animIn)
            ).Then(
                d => d.ScaleTo(1, 150, Easing.Out)
            );
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(FadeOutTime - sliderBouncer.StartTime).FadeOut();
                    break;
                case ArmedState.Miss:
                    this.FadeOut(160);
                    break;
                case ArmedState.Hit:
                    this.FadeOut(120, Easing.OutQuint)
                        .ScaleTo(Scale * 1.5f, 120, Easing.OutQuint);
                    break;
            }
        }
    }
}
