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
using OpenTK.Graphics.ES30;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

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
                body = new Body(s)
                {
                    Position = s.Position,
                },
                ball = new Ball(),
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

            double currentProgress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);
            double drawStartProgress = 0;

            int currentRepeat = (int)(currentProgress * slider.RepeatCount);
            currentProgress = (currentProgress * slider.RepeatCount) % 1;
            if (currentRepeat % 2 == 1)
                currentProgress = 1 - currentProgress;

            if (currentRepeat >= slider.RepeatCount - 1)
                drawStartProgress = currentProgress;

            ball.Position = slider.Curve.PositionAt(currentProgress);

            double drawEndProgress = MathHelper.Clamp((Time.Current - slider.StartTime + TIME_PREEMPT) / TIME_FADEIN, 0, 1);
            body.SetRange(drawStartProgress, drawEndProgress);
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
            private BufferedContainer container;

            private double drawnProgressStart;
            private double drawnProgressEnd;

            private Slider slider;
            public Body(Slider s)
            {
                slider = s;

                Children = new Drawable[]
                {
                    container = new BufferedContainer
                    {
                        CacheDrawnFrameBuffer = true,
                        Children = new Drawable[]
                        {
                            path = new Path
                            {
                                Colour = s.Colour,
                                BlendingMode = BlendingMode.None,
                            },
                        }
                    }
                };

                container.Attach(RenderbufferInternalFormat.DepthComponent16);
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                // Surprisingly, this looks somewhat okay and works well as a test for self-overlaps.
                // TODO: Don't do this.
                path.Texture = textures.Get(@"Menu/logo");
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                path.PathWidth = 32;
            }

            public void SetRange(double p0, double p1)
            {
                if (p0 > p1)
                    MathHelper.Swap(ref p0, ref p1);

                if (updateSnaking(p0, p1))
                {
                    // Autosizing does not give us the desired behaviour here.
                    // We want the container to have the same size as the slider,
                    // and to be positioned such that the slider head is at (0,0).
                    container.Size = path.Size;
                    container.Position = -path.PositionInBoundingBox(slider.Curve.PositionAt(0) - currentCurve[0]);

                    container.ForceRedraw();
                }
            }

            private List<Vector2> currentCurve = new List<Vector2>();
            private bool updateSnaking(double p0, double p1)
            {
                if (drawnProgressStart == p0 && drawnProgressEnd == p1) return false;

                drawnProgressStart = p0;
                drawnProgressEnd = p1;

                slider.Curve.GetPathToProgress(currentCurve, p0, p1);

                path.ClearVertices();
                foreach (Vector2 p in currentCurve)
                    path.AddVertex(p - currentCurve[0]);

                return true;
            }
        }
    }
}
