// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Screens;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Settings
{
    public class TestSceneLatencyComparer : ScreenTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("Load screen", () => LoadScreen(new LatencyComparerScreen()));
        }
    }

    public class LatencyComparerScreen : OsuScreen
    {
        private FrameSync previousFrameSyncMode;

        public override bool HideOverlaysOnEnter => true;

        public override bool CursorVisible => false;

        [Resolved]
        private FrameworkConfigManager config { get; set; } = null!;

        public LatencyComparerScreen()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 100),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 100),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            // header content
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new LatencyArea(10)
                                    {
                                        Width = 0.5f,
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                    },
                                    new LatencyArea(0)
                                    {
                                        Width = 0.5f,
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                    },
                                    new Box
                                    {
                                        Colour = Color4.Black,
                                        Width = 50,
                                        RelativeSizeAxes = Axes.Y,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                    },
                                }
                            },
                        },
                        new Drawable[]
                        {
                            // footer content
                        },
                    }
                }
            };
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            previousFrameSyncMode = config.Get<FrameSync>(FrameworkSetting.FrameSync);
            config.SetValue(FrameworkSetting.FrameSync, FrameSync.Unlimited);
            // host.AllowBenchmarkUnlimitedFrames = true;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // host.AllowBenchmarkUnlimitedFrames = false;
            config.SetValue(FrameworkSetting.FrameSync, previousFrameSyncMode);
            return base.OnExiting(e);
        }

        public class LatencyArea : CompositeDrawable
        {
            private readonly int inducedLatency;

            public LatencyArea(int inducedLatency)
            {
                this.inducedLatency = inducedLatency;
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = Color4.Blue,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new LatencyMovableBox
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new LatencyCursorContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                };
            }

            private long frameCount;

            public override bool UpdateSubTree()
            {
                if (inducedLatency > 0 && ++frameCount % inducedLatency != 0)
                    return false;

                return base.UpdateSubTree();
            }

            public class LatencyMovableBox : CompositeDrawable
            {
                private Box box = null!;
                private InputManager inputManager = null!;

                public LatencyMovableBox()
                {
                    Masking = true;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    inputManager = GetContainingInputManager();

                    InternalChild = box = new Box
                    {
                        Size = new Vector2(40),
                        Position = DrawSize / 2,
                        Origin = Anchor.Centre,
                    };
                }

                protected override bool OnHover(HoverEvent e) => false;

                private double? lastFrameTime;

                protected override void Update()
                {
                    base.Update();

                    if (!IsHovered)
                        return;

                    if (lastFrameTime != null)
                    {
                        float movementAmount = (float)(Clock.CurrentTime - lastFrameTime);

                        foreach (var key in inputManager.CurrentState.Keyboard.Keys)
                        {
                            switch (key)
                            {
                                case Key.Up:
                                    box.Y -= movementAmount;
                                    break;

                                case Key.Down:
                                    box.Y += movementAmount;
                                    break;

                                case Key.Left:
                                    box.X -= movementAmount;
                                    break;

                                case Key.Right:
                                    box.X += movementAmount;
                                    break;
                            }
                        }
                    }

                    lastFrameTime = Clock.CurrentTime;
                }
            }

            public class LatencyCursorContainer : CompositeDrawable
            {
                private readonly Circle cursor;
                private InputManager inputManager = null!;

                public LatencyCursorContainer()
                {
                    Masking = true;

                    InternalChild = cursor = new Circle
                    {
                        Size = new Vector2(40),
                        Origin = Anchor.Centre,
                    };
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    inputManager = GetContainingInputManager();
                }

                protected override bool OnHover(HoverEvent e) => false;

                protected override void Update()
                {
                    if (IsHovered)
                    {
                        cursor.Position = ToLocalSpace(inputManager.CurrentState.Mouse.Position);
                        cursor.Alpha = 1;
                    }
                    else
                    {
                        cursor.Alpha = 0;
                    }

                    base.Update();
                }
            }
        }
    }
}
