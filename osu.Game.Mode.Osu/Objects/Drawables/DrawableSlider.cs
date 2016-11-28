using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        private Slider slider;

        private DrawableHitCircle startCircle;
        private Container ball;
        private SliderBody body;

        public DrawableSlider(Slider s) : base(s)
        {
            slider = s;

            Origin = Anchor.TopLeft;
            Position = Vector2.Zero;

            Children = new Drawable[]
            {
                body = new SliderBody(s),
                ball = new Container
                {
                    Masking = true,
                    CornerRadius = 20,
                    AutoSizeAxes = Axes.Both,
                    Colour = Color4.Red,
                    Origin = Anchor.Centre,
                    Children = new []
                    {
                        new Box
                        {
                            Width = 40,
                            Height = 40,
                        }
                    }
                },
                startCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.Position,
                    Colour = s.Colour,
                })
                {
                    Depth = 1 //override time-based depth.
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void Update()
        {
            base.Update();

            ball.Alpha = Time.Current >= slider.StartTime && Time.Current <= slider.EndTime ? 1 : 0;

            double t = (Time.Current - slider.StartTime) / slider.Duration;
            if (slider.RepeatCount > 1)
            {
                double currentRepeat = (int)(t * slider.RepeatCount);
                t = (t * slider.RepeatCount) % 1;
                if (currentRepeat % 2 == 1)
                    t = 1 - t;
            }

            ball.Position = slider.Curve.PositionAt(t);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject

            Alpha = 0;

            Delay(HitObject.StartTime - 450 - Time.Current, true);

            FadeIn(300);
            Delay(450 + HitObject.Duration);
            FadeOut(100);
        }

        class SliderBody : Container
        {
            private Path path;

            double? drawnProgress;

            Slider slider;

            public SliderBody(Slider s)
            {
                slider = s;

                Children = new Drawable[]
                {
                    //new BufferedContainer
                    //{
                    //    RelativeSizeAxes = Axes.Both,
                    //    Children = new Drawable[]
                    //    {
                            path = new Path
                            {
                                Colour = s.Colour,
                            },
                    //    }
                    //}
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                path.PathWidth = 50;
            }

            protected override void Update()
            {
                base.Update();

                double segmentSize = 1 / (slider.Curve.Length / 5);
                double progress = MathHelper.Clamp((Time.Current - slider.StartTime + TIME_PREEMPT) / TIME_FADEIN, 0, 1);

                if (progress != drawnProgress)
                {
                    if (progress == 0)
                    {
                        //if we have gone backwards, just clear the path for now.
                        drawnProgress = 0;
                        path.Positions.Clear();
                    }

                    if (drawnProgress == null)
                    {
                        drawnProgress = 0;
                        path.Positions.Add(slider.Curve.PositionAt(drawnProgress.Value));
                    }

                    while (drawnProgress + segmentSize < progress)
                    {
                        drawnProgress += segmentSize;
                        path.Positions.Add(slider.Curve.PositionAt(drawnProgress.Value));
                    }

                    if (progress == 1 && drawnProgress != progress)
                    {
                        drawnProgress = progress;
                        path.Positions.Add(slider.Curve.PositionAt(drawnProgress.Value));
                    }

                    path.Invalidate(Invalidation.DrawNode);
                }
            }
        }
    }
}
