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

        private readonly SliderBody body;
        private readonly SliderBall ball;

        private readonly SliderBouncer bouncer2;

        public DrawableSlider(Slider s) : base(s)
        {
            SliderBouncer bouncer1;
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
                bouncer1 = new SliderBouncer(s, false)
                {
                    Position = s.Curve.PositionAt(1),
                    Scale = new Vector2(s.Scale),
                },
                bouncer2 = new SliderBouncer(s, true)
                {
                    Position = s.StackedPosition,
                    Scale = new Vector2(s.Scale),
                },
                ball = new SliderBall(s)
                {
                    Scale = new Vector2(s.Scale),
                    AccentColour = AccentColour
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
            components.Add(bouncer1);
            components.Add(bouncer2);

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
        }

        private int currentRepeat;

        protected override void Update()
        {
            base.Update();

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int repeat = slider.RepeatAt(progress);
            progress = slider.ProgressAt(progress);

            if (repeat > currentRepeat)
            {
                if (repeat < slider.RepeatCount && ball.Tracking)
                    PlaySamples();
                currentRepeat = repeat;
            }

            bouncer2.Position = slider.Curve.PositionAt(body.SnakedEnd ?? 0);

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
                var ticksCount = ticks.Children.Count + 1;
                var ticksHit = ticks.Children.Count(t => t.Judgements.Any(j => j.IsHit));
                if (initialCircle.Judgements.Any(j => j.IsHit))
                    ticksHit++;

                var hitFraction = (double)ticksHit / ticksCount;
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

        protected override void UpdateInitialState()
        {
            base.UpdateInitialState();
            body.Alpha = 1;

            //we need to be present to handle input events. note that we still don't get enough events (we don't get a position if the mouse hasn't moved since the slider appeared).
            ball.AlwaysPresent = true;
            ball.Alpha = 0;
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
