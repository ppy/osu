// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableRepeatPoint : DrawableOsuHitObject
    {
        private readonly RepeatPoint repeatPoint;
        private readonly DrawableSlider drawableSlider;

        public double FadeInTime;
        public double FadeOutTime;

        public override bool RemoveWhenNotAlive => false;

        public DrawableRepeatPoint(RepeatPoint repeatPoint, DrawableSlider drawableSlider) : base(repeatPoint)
        {
            this.repeatPoint = repeatPoint;
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
            if (repeatPoint.StartTime <= Time.Current)
                AddJudgement(new OsuJudgement { Result = drawableSlider.Tracking ? HitResult.Great : HitResult.Miss });
        }

        protected override void UpdatePreemptState()
        {
            var animIn = Math.Min(150, repeatPoint.StartTime - FadeInTime);

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
