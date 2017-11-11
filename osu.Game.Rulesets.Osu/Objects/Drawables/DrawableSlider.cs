// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private readonly Slider slider;

        private readonly DrawableHitCircle initialCircle;

        private readonly List<ISliderProgress> components = new List<ISliderProgress>();

        private readonly Container<DrawableSliderTick> ticks;
        private readonly Container<DrawableRepeatPoint> repeatPoints;

        private readonly SliderBody body;
        private readonly SliderBall ball;

        public DrawableSlider(Slider s) : base(s)
        {
            slider = s;

            Children = new Drawable[]
            {
                body = new SliderBody(s)
                {
                    AccentColour = AccentColour,
                    Position = s.StackedPosition,
                    PathWidth = s.Scale * 64,
                },
                ticks = new Container<DrawableSliderTick>(),
                repeatPoints = new Container<DrawableRepeatPoint>(),
                ball = new SliderBall(s)
                {
                    Scale = new Vector2(s.Scale),
                    AccentColour = AccentColour,
                    AlwaysPresent = true,
                    Alpha = 0
                },
                initialCircle = new DrawableHitCircle(new HitCircle
                {
                    //todo: avoid creating this temporary HitCircle.
                    StartTime = s.StartTime,
                    Position = s.StackedPosition,
                    ComboIndex = s.ComboIndex,
                    Scale = s.Scale,
                    ComboColour = s.ComboColour,
                    Samples = s.Samples,
                })
            };

            components.Add(body);
            components.Add(ball);

            AddNested(initialCircle);

            var repeatDuration = s.Curve.Distance / s.Velocity;
            foreach (var tick in s.Ticks)
            {
                var repeatStartTime = s.StartTime + tick.RepeatIndex * repeatDuration;
                var fadeInTime = repeatStartTime + (tick.StartTime - repeatStartTime) / 2 - (tick.RepeatIndex == 0 ? TIME_FADEIN : TIME_FADEIN / 2);
                var fadeOutTime = repeatStartTime + repeatDuration;

                var drawableTick = new DrawableSliderTick(tick)
                {
                    FadeInTime = fadeInTime,
                    FadeOutTime = fadeOutTime,
                    Position = tick.Position,
                };

                ticks.Add(drawableTick);
                AddNested(drawableTick);
            }

            foreach (var repeatPoint in s.RepeatPoints)
            {
                var repeatStartTime = s.StartTime + repeatPoint.RepeatIndex * repeatDuration;
                var fadeInTime = repeatStartTime + (repeatPoint.StartTime - repeatStartTime) / 2 - (repeatPoint.RepeatIndex == 0 ? TIME_FADEIN : TIME_FADEIN / 2);
                var fadeOutTime = repeatStartTime + repeatDuration;

                var drawableRepeatPoint = new DrawableRepeatPoint(repeatPoint, this)
                {
                    FadeInTime = fadeInTime,
                    FadeOutTime = fadeOutTime,
                    Position = repeatPoint.Position,
                };

                repeatPoints.Add(drawableRepeatPoint);
                AddNested(drawableRepeatPoint);
            }
        }

        private int currentRepeat;
        public bool Tracking;

        protected override void Update()
        {
            base.Update();

            Tracking = ball.Tracking;

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int repeat = slider.RepeatAt(progress);
            progress = slider.ProgressAt(progress);

            if (repeat > currentRepeat)
            {
                if (repeat < slider.RepeatCount && ball.Tracking)
                    PlaySamples();
                currentRepeat = repeat;
            }

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (!initialCircle.Judgements.Any(j => j.IsHit))
                initialCircle.Position = slider.Curve.PositionAt(progress);

            foreach (var c in components) c.UpdateProgress(progress, repeat);
            foreach (var t in ticks.Children) t.Tracking = ball.Tracking;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered && Time.Current >= slider.EndTime)
            {
                var judgementsCount = ticks.Children.Count + repeatPoints.Children.Count + 1;
                var judgementsHit = ticks.Children.Count(t => t.Judgements.Any(j => j.IsHit)) + repeatPoints.Children.Count(t => t.Judgements.Any(j => j.IsHit));
                if (initialCircle.Judgements.Any(j => j.IsHit))
                    judgementsHit++;

                var hitFraction = (double)judgementsHit / judgementsCount;
                if (hitFraction == 1 && initialCircle.Judgements.Any(j => j.Result == HitResult.Great))
                    AddJudgement(new OsuJudgement { Result = HitResult.Great });
                else if (hitFraction >= 0.5 && initialCircle.Judgements.Any(j => j.Result >= HitResult.Good))
                    AddJudgement(new OsuJudgement { Result = HitResult.Good });
                else if (hitFraction > 0)
                    AddJudgement(new OsuJudgement { Result = HitResult.Meh });
                else
                    AddJudgement(new OsuJudgement { Result = HitResult.Miss });
            }
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            ball.FadeIn();

            using (BeginDelayedSequence(slider.Duration, true))
            {
                body.FadeOut(160);
                ball.FadeOut(160);

                this.FadeOut(800)
                    .Expire();
            }
        }

        public Drawable ProxiedLayer => initialCircle.ApproachCircle;
    }

    internal interface ISliderProgress
    {
        void UpdateProgress(double progress, int repeat);
    }
}
