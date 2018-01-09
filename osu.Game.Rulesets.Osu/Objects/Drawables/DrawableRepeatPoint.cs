// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableRepeatPoint : DrawableOsuHitObject
    {
        private readonly RepeatPoint repeatPoint;
        private readonly DrawableSlider drawableSlider;

        public double FadeInTime;
        public double FadeOutTime;

        public DrawableRepeatPoint(RepeatPoint repeatPoint, DrawableSlider drawableSlider)
            : base(repeatPoint)
        {
            this.repeatPoint = repeatPoint;
            this.drawableSlider = drawableSlider;

            Size = new Vector2(32 * repeatPoint.Scale);

            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_eercast
                }
            };
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (repeatPoint.StartTime <= Time.Current)
                AddJudgement(new OsuJudgement { Result = drawableSlider.Tracking ? HitResult.Great : HitResult.Miss });
        }

        protected override void UpdatePreemptState()
        {
            var animIn = Math.Min(150, repeatPoint.StartTime - FadeInTime);

            this.FadeIn(animIn).ScaleTo(1.2f, animIn)
                .Then()
                .ScaleTo(1, 150, Easing.Out);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(FadeOutTime - repeatPoint.StartTime).FadeOut();
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
