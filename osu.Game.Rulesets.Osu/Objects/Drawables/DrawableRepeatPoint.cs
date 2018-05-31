// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
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

        private double animDuration;

        public DrawableRepeatPoint(RepeatPoint repeatPoint, DrawableSlider drawableSlider)
            : base(repeatPoint)
        {
            this.repeatPoint = repeatPoint;
            this.drawableSlider = drawableSlider;

            Size = new Vector2(45 * repeatPoint.Scale);

            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
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

            this.Animate(
                d => d.FadeIn(animDuration),
                d => d.ScaleTo(0.5f).ScaleTo(1f, animDuration * 4, Easing.OutElasticHalf)
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
                    this.FadeOut(animDuration);
                    break;
                case ArmedState.Hit:
                    this.FadeOut(animDuration, Easing.OutQuint)
                        .ScaleTo(Scale * 1.5f, animDuration, Easing.Out);
                    break;
            }
        }

        public void UpdateSnakingPosition(Vector2 start, Vector2 end)
        {
            bool isRepeatAtEnd = repeatPoint.RepeatIndex % 2 == 0;
            List<Vector2> curve = drawableSlider.Body.CurrentCurve;

            Position = isRepeatAtEnd ? end : start;

            if (curve.Count < 2)
                return;

            int searchStart = isRepeatAtEnd ? curve.Count - 1 : 0;
            int direction = isRepeatAtEnd ? -1 : 1;

            // find the next vector2 in the curve which is not equal to our current position to infer a rotation.
            for (int i = searchStart; i >= 0 && i < curve.Count; i += direction)
            {
                if (curve[i] == Position)
                    continue;

                Rotation = MathHelper.RadiansToDegrees((float)Math.Atan2(curve[i].Y - Position.Y, curve[i].X - Position.X));
                break;
            }
        }
    }
}
