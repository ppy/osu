// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableRepeatPoint : DrawableOsuHitObject, ITrackSnaking
    {
        private readonly RepeatPoint repeatPoint;
        private readonly DrawableSlider drawableSlider;

        /// <summary>
        /// Are we located in the last ControlPoint of our <see cref="DrawableSlider.CurrentCurve"/>
        /// </summary>
        private bool isRepeatAtEnd => repeatPoint.RepeatIndex % 2 == 0;

        private double animDuration;

        public DrawableRepeatPoint(RepeatPoint repeatPoint, DrawableSlider drawableSlider)
            : base(repeatPoint)
        {
            this.repeatPoint = repeatPoint;
            this.drawableSlider = drawableSlider;

            Size = new Vector2(45 * repeatPoint.Scale);

            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_chevron_right
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
            animDuration = Math.Min(150, repeatPoint.SpanDuration / 2);

            this.FadeIn(animDuration).ScaleTo(1.2f, animDuration / 2)
                .Then()
                .ScaleTo(1, animDuration / 2, Easing.Out);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(HitObject.TimePreempt).FadeOut();
                    break;
                case ArmedState.Miss:
                    this.FadeOut(animDuration);
                    break;
                case ArmedState.Hit:
                    this.FadeOut(animDuration, Easing.OutQuint)
                        .ScaleTo(Scale * 1.5f, animDuration, Easing.OutQuint);
                    break;
            }
        }

        public void UpdateSnakingPosition(Vector2 start, Vector2 end)
        {
            Position = isRepeatAtEnd ? end : start;
            var curve = drawableSlider.CurrentCurve;
            if (curve.Count < 3 || curve.All(p => p == Position))
                return;
            var referencePoint = curve[isRepeatAtEnd ? curve.IndexOf(Position, curve.Count - 2) - 1 : curve[0] == curve[1] ? 2 : 1];
            Rotation = MathHelper.RadiansToDegrees((float)Math.Atan2(referencePoint.Y - Position.Y, referencePoint.X - Position.X));
        }
    }
}
