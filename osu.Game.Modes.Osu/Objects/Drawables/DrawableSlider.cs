// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableSlider : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private Slider slider;

        private DrawableHitCircle initialCircle;

        private List<ISliderProgress> components = new List<ISliderProgress>();

        private Container<DrawableSliderTick> ticks;

        SliderBody body;
        SliderBall ball;

        SliderBouncer bouncer1, bouncer2;

        public DrawableSlider(Slider s) : base(s)
        {
            slider = s;

            Children = new Drawable[]
            {
                body = new SliderBody(s)
                {
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
                },
                initialCircle = new DrawableHitCircle(new HitCircle
                {
                    //todo: avoid creating this temporary HitCircle.
                    StartTime = s.StartTime,
                    Position = s.StackedPosition,
                    ComboIndex = s.ComboIndex,
                    Scale = s.Scale,
                    Colour = s.Colour,
                    Sample = s.Sample,
                }),
            };

            components.Add(body);
            components.Add(ball);
            components.Add(bouncer1);
            components.Add(bouncer2);

            AddNested(initialCircle);

            var repeatDuration = s.Curve.Length / s.Velocity;
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

        // Since the DrawableSlider itself is just a container without a size we need to
        // pass all input through.
        public override bool Contains(Vector2 screenSpacePos) => true;

        int currentRepeat;

        protected override void Update()
        {
            base.Update();

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int repeat = (int)(progress * slider.RepeatCount);
            progress = (progress * slider.RepeatCount) % 1;

            if (repeat > currentRepeat)
            {
                if (repeat < slider.RepeatCount && ball.Tracking)
                    PlaySample();
                currentRepeat = repeat;
            }

            if (repeat % 2 == 1)
                progress = 1 - progress;

            bouncer2.Position = slider.Curve.PositionAt(body.SnakedEnd ?? 0);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (initialCircle.Judgement?.Result != HitResult.Hit)
                initialCircle.Position = slider.Curve.PositionAt(progress);

            foreach (var c in components) c.UpdateProgress(progress, repeat);
            foreach (var t in ticks.Children) t.Tracking = ball.Tracking;
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            var j = Judgement as OsuJudgementInfo;
            var sc = initialCircle.Judgement as OsuJudgementInfo;

            if (!userTriggered && Time.Current >= HitObject.EndTime)
            {
                var ticksCount = ticks.Children.Count() + 1;
                var ticksHit = ticks.Children.Count(t => t.Judgement.Result == HitResult.Hit);
                if (sc.Result == HitResult.Hit)
                    ticksHit++;

                var hitFraction = (double)ticksHit / ticksCount;
                if (hitFraction == 1 && sc.Score == OsuScoreResult.Hit300)
                    j.Score = OsuScoreResult.Hit300;
                else if (hitFraction >= 0.5 && sc.Score >= OsuScoreResult.Hit100)
                    j.Score = OsuScoreResult.Hit100;
                else if (hitFraction > 0)
                    j.Score = OsuScoreResult.Hit50;
                else
                    j.Score = OsuScoreResult.Miss;

                j.Result = j.Score != OsuScoreResult.Miss ? HitResult.Hit : HitResult.Miss;
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

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            ball.FadeIn();

            Delay(HitObject.Duration, true);

            body.FadeOut(160);
            ball.FadeOut(160);

            FadeOut(800);
        }

        public Drawable ProxiedLayer => initialCircle.ApproachCircle;
    }

    internal interface ISliderProgress
    {
        void UpdateProgress(double progress, int repeat);
    }
}