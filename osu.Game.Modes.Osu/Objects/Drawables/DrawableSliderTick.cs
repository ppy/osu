// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using System;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class DrawableSliderTick : DrawableOsuHitObject
    {
        private SliderTick sliderTick;

        public double FadeInTime;
        public double FadeOutTime;

        public bool ShouldHit;

        public DrawableSliderTick(SliderTick sliderTick) : base(sliderTick)
        {
            this.sliderTick = sliderTick;

            Size = new Vector2(16) * sliderTick.Scale;

            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            BorderThickness = 2;
            BorderColour = Color4.White;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = sliderTick.Colour,
                    Alpha = 0.3f,
                }
            };
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            if (Judgement.TimeOffset >= 0)
                Judgement.Result = ShouldHit ? HitResult.Hit : HitResult.Miss;
        }
        
        protected override void UpdatePreemptState()
        {
            var animIn = Math.Min(150, sliderTick.StartTime - FadeInTime);

            ScaleTo(0.5f);
            ScaleTo(1.2f, animIn);
            FadeIn(animIn);

            Delay(animIn);
            ScaleTo(1, 150, EasingTypes.Out);

            Delay(-animIn);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            base.UpdateState(state);

            switch (state)
            {
                case ArmedState.Idle:
                    Delay(FadeOutTime - sliderTick.StartTime);
                    FadeOut();
                    break;
                case ArmedState.Miss:
                    FadeTo(0.6f);
                    Delay(FadeOutTime - sliderTick.StartTime);
                    FadeOut();
                    break;
                case ArmedState.Hit:
                    FadeOut();
                    break;
            }
        }
    }
}