// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using OpenTK.Graphics;
using osu.Framework.Input;
using OpenTK.Graphics.ES30;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Framework.Configuration;
using System;

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

        private Bindable<bool> snakingIn;
        private Bindable<bool> snakingOut;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            snakingIn = config.GetBindable<bool>(OsuConfig.SnakingInSliders);
            snakingOut = config.GetBindable<bool>(OsuConfig.SnakingOutSliders);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //force application of the state that was set before we loaded.
            UpdateState(State);

            body.PathWidth = 32;
        }

        private void computeProgress(out int repeat, out double progress)
        {
            progress = MathHelper.Clamp((Time.Current - slider.StartTime) / slider.Duration, 0, 1);

            repeat = (int)(progress * slider.RepeatCount);
            progress = (progress * slider.RepeatCount) % 1;

            if (repeat % 2 == 1)
                progress = 1 - progress;
        }

        private void updateBall(double progress)
        {
            ball.Alpha = Time.Current >= slider.StartTime && Time.Current <= slider.EndTime ? 1 : 0;
            ball.Position = slider.Curve.PositionAt(progress);
        }

        private void updateBody(int repeat, double progress)
        {
            double drawStartProgress = 0;
            double drawEndProgress = MathHelper.Clamp((Time.Current - slider.StartTime + TIME_PREEMPT) / TIME_FADEIN, 0, 1);

            if (repeat >= slider.RepeatCount - 1)
            {
                if (Math.Min(repeat, slider.RepeatCount - 1) % 2 == 1)
                {
                    drawStartProgress = 0;
                    drawEndProgress = progress;
                }
                else
                {
                    drawStartProgress = progress;
                    drawEndProgress = 1;
                }
            }

            body.SetRange(
                snakingOut ? drawStartProgress : 0,
                snakingIn ? drawEndProgress : 1);
        }

        protected override void Update()
        {
            base.Update();

            double progress;
            int repeat;
            computeProgress(out repeat, out progress);

            updateBall(progress);
            updateBody(repeat, progress);
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

            public float PathWidth
            {
                get { return path.PathWidth; }
                set { path.PathWidth = value; }
            }

            private double? drawnProgressStart;
            private double? drawnProgressEnd;

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
