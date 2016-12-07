//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        private Slider slider;

        private DrawableHitCircle initialCircle;

        private List<ISliderProgress> components = new List<ISliderProgress>();

        SliderBody body;

        SliderBouncer bouncer1, bouncer2;

        public DrawableSlider(Slider s) : base(s)
        {
            SliderBall ball;

            slider = s;

            Origin = Anchor.TopLeft;
            Position = Vector2.Zero;
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                body = new SliderBody(s)
                {
                    Position = s.Position,
                    PathWidth = 36,
                },
                bouncer1 = new SliderBouncer(slider, false) { Position = slider.Curve.PositionAt(1) },
                bouncer2 = new SliderBouncer(slider, true) { Position = slider.Position },
                ball = new SliderBall(slider),
                initialCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.Position,
                    Colour = s.Colour,
                })
                {
                    Depth = -1 //override time-based depth.
                },
            };

            components.Add(body);
            components.Add(ball);
            components.Add(bouncer1);
            components.Add(bouncer2);
        }

        protected override void Update()
        {
            base.Update();

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int repeat = (int)(progress * slider.RepeatCount);
            progress = (progress * slider.RepeatCount) % 1;

            if (repeat % 2 == 1)
                progress = 1 - progress;

            bouncer2.Position = slider.Curve.PositionAt(body.SnakedEnd ?? 0);

            //todo: we probably want to reconsider this before adding scoring, but it looks and feels nice.
            if (initialCircle.Judgement?.Result != HitResult.Hit)
                initialCircle.Position = slider.Curve.PositionAt(body.SnakedStart ?? 0);

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
        }

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            Delay(HitObject.Duration, true);
            body.FadeOut(160);
            FadeOut(800);
        }
    }

    internal interface ISliderProgress
    {
        void UpdateProgress(double progress, int repeat);
    }
}