using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using OpenTK.Graphics;
using osu.Framework.Input;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    class DrawableSlider : DrawableOsuHitObject
    {
        private Slider slider;

        private DrawableHitCircle startCircle;
        private Container ball;
        private Body body;

        public DrawableSlider(Slider s) : base(s)
        {
            slider = s;

            Origin = Anchor.TopLeft;
            Position = Vector2.Zero;
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                body = new Body(s),
                ball = new Ball(),
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
            base.UpdateState(state);

            Delay(HitObject.Duration);
            FadeOut(100);
        }

        private class Ball : Container
        {
            private Box follow;

            public Ball()
            {
                Masking = true;
                AutoSizeAxes = Axes.Both;
                BlendingMode = BlendingMode.Additive;
                Origin = Anchor.Centre;

                Children = new Drawable[]
                {
                    follow = new Box
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Colour = Color4.Orange,
                        Width = 64,
                        Height = 64,
                    },
                    new Container
                    {
                        Masking = true,
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Colour = Color4.Cyan,
                        CornerRadius = 32,
                        Children = new[]
                        {
                            new Box
                            {
                                
                                Width = 64,
                                Height = 64,
                            },
                        }
                    }
                    
                };
            }

            private InputState lastState;

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
            {
                lastState = state;
                return base.OnMouseDown(state, args);
            }

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
            {
                lastState = state;
                return base.OnMouseUp(state, args);
            }

            protected override bool OnMouseMove(InputState state)
            {
                lastState = state;
                return base.OnMouseMove(state);
            }

            bool tracking;
            protected bool Tracking
            {
                get { return tracking; }
                set
                {
                    if (value == tracking) return;

                    tracking = value;

                    follow.ScaleTo(tracking ? 2.4f : 1, 140, EasingTypes.Out);
                    follow.FadeTo(tracking ? 0.8f : 0, 140, EasingTypes.Out);
                }
            }
            
            protected override void Update()
            {
                base.Update();

                CornerRadius = DrawWidth / 2;
                Tracking = lastState != null && Contains(lastState.Mouse.NativeState.Position) && lastState.Mouse.HasMainButtonPressed;
            }
        }

        private class Body : Container
        {
            private Path path;

            private double? drawnProgress;

            private Slider slider;

            public Body(Slider s)
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
                path.PathWidth = 32;
            }

            protected override void Update()
            {
                base.Update();

                updateSnaking();
            }

            private void updateSnaking()
            {
                double progress = MathHelper.Clamp((Time.Current - slider.StartTime + TIME_PREEMPT / 2) / TIME_FADEIN, 0, 1);

                if (progress == drawnProgress) return;

                if (progress == 0)
                {
                    //if we have gone backwards, just clear the path for now.
                    drawnProgress = 0;
                    path.ClearVertices();
                }

                if (drawnProgress == null)
                {
                    drawnProgress = 0;
                    path.AddVertex(slider.Curve.PositionAt(drawnProgress.Value));
                }

                double segmentSize = 1 / (slider.Curve.Length / 5);

                while (drawnProgress + segmentSize < progress)
                {
                    drawnProgress += segmentSize;
                    path.AddVertex(slider.Curve.PositionAt(drawnProgress.Value));
                }

                if (progress == 1 && drawnProgress != progress)
                {
                    drawnProgress = progress;
                    path.AddVertex(slider.Curve.PositionAt(drawnProgress.Value));
                }

                path.Invalidate(Invalidation.DrawNode);
            }
        }
    }
}
