//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        private Slider slider;

        private DrawableHitCircle startCircle;

        private List<ISliderProgress> components = new List<ISliderProgress>();

        public DrawableSlider(Slider s) : base(s)
        {
            SliderBody body;
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
                ball = new SliderBall(slider),
                startCircle = new DrawableHitCircle(new HitCircle
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
        }

        protected override void Update()
        {
            base.Update();

            double progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            int repeat = (int)(progress * slider.RepeatCount);
            progress = (progress * slider.RepeatCount) % 1;

            if (repeat % 2 == 1)
                progress = 1 - progress;

            components.ForEach(c => c.UpdateProgress(progress, repeat));
        }

        protected override void CheckJudgement(bool userTriggered)
        {
            var j = Judgement as OsuJudgementInfo;
            var sc = startCircle.Judgement as OsuJudgementInfo;

            if (!userTriggered && Time.Current >= HitObject.EndTime)
            {
                j.Score = sc.Score;
                j.Result = sc.Result;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            Delay(HitObject.Duration);
            FadeOut(300);
        }
    }

    internal interface ISliderProgress
    {
        void UpdateProgress(double progress, int repeat);
    }
}