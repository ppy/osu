//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Input;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        private Slider slider;

        private DrawableHitCircle initialCircle;

        private List<ISliderProgress> components = new List<ISliderProgress>();

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
                    Position = s.Position,
                    PathWidth = s.Scale * 72,
                },
                bouncer1 = new SliderBouncer(s, false)
                {
                    Position = s.Curve.PositionAt(1),
                    Scale = new Vector2(s.Scale),
                },
                bouncer2 = new SliderBouncer(s, true)
                {
                    Position = s.Position,
                    Scale = new Vector2(s.Scale),
                },
                ball = new SliderBall(s)
                {
                    Scale = new Vector2(s.Scale),
                },
                initialCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.Position,
                    Scale = s.Scale,
                    Colour = s.Colour,
                    Sample = s.Sample,
                }),
            };

            components.Add(body);
            components.Add(ball);
            components.Add(bouncer1);
            components.Add(bouncer2);
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
                if (ball.Tracking)
                    PlaySample();
                currentRepeat = repeat;
            }

            if (repeat % 2 == 1)
                progress = 1 - progress;

            bouncer2.Position = slider.Curve.PositionAt(body.SnakedEnd ?? 0);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (initialCircle.Judgement?.Result != HitResult.Hit)
                initialCircle.Position = slider.Curve.PositionAt(progress);

            components.ForEach(c => c.UpdateProgress(progress, repeat));
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            var j = Judgement as OsuJudgementInfo;
            var sc = initialCircle.Judgement as OsuJudgementInfo;

            if (!userTriggered && Time.Current >= HitObject.EndTime)
            {
                j.Score = sc.Score;
                j.Result = sc.Result;
            }
        }

        protected override void UpdateInitialState()
        {
            base.UpdateInitialState();
            body.Alpha = 1;

            //we need to be visible to handle input events. note that we still don't get enough events (we don't get a position if the mouse hasn't moved since the slider appeared).
            ball.Alpha = 0.01f; 
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
    }

    internal interface ISliderProgress
    {
        void UpdateProgress(double progress, int repeat);
    }
}