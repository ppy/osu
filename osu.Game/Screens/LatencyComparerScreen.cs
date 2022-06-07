// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens
{
    public class LatencyComparerScreen : OsuScreen
    {
        private FrameSync previousFrameSyncMode;

        private readonly OsuTextFlowContainer statusText;

        public override bool HideOverlaysOnEnter => true;

        public override bool CursorVisible => false;

        public override float BackgroundParallaxAmount => 0;

        private readonly Container mainArea;

        private readonly Container resultsArea;

        [Cached]
        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Orange);

        [Resolved]
        private FrameworkConfigManager config { get; set; } = null!;

        public LatencyComparerScreen()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColourProvider.Background6,
                    RelativeSizeAxes = Axes.Both,
                },
                mainArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // Make sure the edge between the two comparisons can't be used to ascertain latency.
                new Box
                {
                    Name = "separator",
                    Colour = ColourInfo.GradientHorizontal(overlayColourProvider.Background6, overlayColourProvider.Background6.Opacity(0)),
                    Width = 50,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft,
                },
                new Box
                {
                    Name = "separator",
                    Colour = ColourInfo.GradientHorizontal(overlayColourProvider.Background6.Opacity(0), overlayColourProvider.Background6),
                    Width = 50,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight,
                },
                statusText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 40))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextAnchor = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                resultsArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loadNextRound();
        }

        private int round;

        private const int rounds_to_complete = 10;

        private int correctCount;
        private int targetRoundCount = rounds_to_complete;

        private void recordResult(bool correct)
        {
            if (correct)
                correctCount++;

            if (round < targetRoundCount)
                loadNextRound();
            else
            {
                showResults();
            }
        }

        private void loadNextRound()
        {
            round++;
            statusText.Text = $"Round {round} of {targetRoundCount}";

            mainArea.Clear();

            const int induced_latency = 1;

            int betterSide = RNG.Next(0, 2);

            mainArea.Add(new LatencyArea(betterSide == 1 ? induced_latency : 0)
            {
                Width = 0.5f,
                ReportBetter = () => recordResult(betterSide == 0)
            });

            mainArea.Add(new LatencyArea(betterSide == 0 ? induced_latency : 0)
            {
                Width = 0.5f,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                ReportBetter = () => recordResult(betterSide == 1)
            });
        }

        private void showResults()
        {
            mainArea.Clear();

            statusText.Text = $"You scored {correctCount} out of {targetRoundCount} ({(float)correctCount / targetRoundCount:P0})!";

            resultsArea.Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Button
                    {
                        Text = "Increase confidence",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Y = 100,
                        Action = () =>
                        {
                            resultsArea.Clear();
                            targetRoundCount += rounds_to_complete;
                            loadNextRound();
                        }
                    }
                }
            });
        }

        public class LatencyArea : CompositeDrawable
        {
            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            public Action? ReportBetter { get; set; }

            private Drawable background = null!;

            private readonly int inducedLatency;

            private Container interactivePieces = null!;

            private Button button = null!;

            private long frameCount;

            public LatencyArea(int inducedLatency)
            {
                this.inducedLatency = inducedLatency;

                RelativeSizeAxes = Axes.Both;
                Masking = true;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                InternalChildren = new[]
                {
                    background = new Box
                    {
                        Colour = overlayColourProvider.Background6,
                        RelativeSizeAxes = Axes.Both,
                    },
                    interactivePieces = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Children = new Drawable[]
                        {
                            new LatencyMovableBox
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                            new LatencyCursorContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    },
                    button = new Button
                    {
                        Text = "Feels better",
                        Alpha = 0,
                        Y = 20,
                        Width = 0.8f,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Action = () => ReportBetter?.Invoke(),
                    },
                };

                base.LoadComplete();
                this.FadeInFromZero(500, Easing.OutQuint);

                using (BeginDelayedSequence(500))
                {
                    interactivePieces.FadeIn(500, Easing.OutQuint);
                    button.FadeIn(500, Easing.OutQuint);
                }
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeColour(overlayColourProvider.Background4, 200, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeColour(overlayColourProvider.Background6, 200, Easing.OutQuint);
                base.OnHoverLost(e);
            }

            public override bool UpdateSubTree()
            {
                if (background?.Alpha == 1 && inducedLatency > 0 && ++frameCount % inducedLatency != 0)
                    return false;

                return base.UpdateSubTree();
            }

            public class LatencyMovableBox : CompositeDrawable
            {
                private Box box = null!;
                private InputManager inputManager = null!;

                [Resolved]
                private OverlayColourProvider overlayColourProvider { get; set; } = null!;

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    inputManager = GetContainingInputManager();

                    InternalChild = box = new Box
                    {
                        Size = new Vector2(40),
                        RelativePositionAxes = Axes.Both,
                        Position = new Vector2(0.5f),
                        Origin = Anchor.Centre,
                        Colour = overlayColourProvider.Colour1,
                    };
                }

                protected override bool OnHover(HoverEvent e) => false;

                private double? lastFrameTime;

                protected override void Update()
                {
                    base.Update();

                    if (!IsHovered)
                    {
                        lastFrameTime = null;
                        return;
                    }

                    if (lastFrameTime != null)
                    {
                        float movementAmount = (float)(Clock.CurrentTime - lastFrameTime) / 400;

                        foreach (var key in inputManager.CurrentState.Keyboard.Keys)
                        {
                            switch (key)
                            {
                                case Key.Up:
                                    box.Y = MathHelper.Clamp(box.Y - movementAmount, 0.1f, 0.9f);
                                    break;

                                case Key.Down:
                                    box.Y = MathHelper.Clamp(box.Y + movementAmount, 0.1f, 0.9f);
                                    break;

                                case Key.Z:
                                case Key.Left:
                                    box.X = MathHelper.Clamp(box.X - movementAmount, 0.1f, 0.9f);
                                    break;

                                case Key.X:
                                case Key.Right:
                                    box.X = MathHelper.Clamp(box.X + movementAmount, 0.1f, 0.9f);
                                    break;
                            }
                        }
                    }

                    lastFrameTime = Clock.CurrentTime;
                }
            }

            public class LatencyCursorContainer : CompositeDrawable
            {
                private Circle cursor = null!;
                private InputManager inputManager = null!;

                [Resolved]
                private OverlayColourProvider overlayColourProvider { get; set; } = null!;

                public LatencyCursorContainer()
                {
                    Masking = true;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    InternalChild = cursor = new Circle
                    {
                        Size = new Vector2(40),
                        Origin = Anchor.Centre,
                        Colour = overlayColourProvider.Colour2,
                    };

                    inputManager = GetContainingInputManager();
                }

                protected override bool OnHover(HoverEvent e) => false;

                protected override void Update()
                {
                    cursor.Colour = inputManager.CurrentState.Mouse.IsPressed(MouseButton.Left) ? overlayColourProvider.Content1 : overlayColourProvider.Colour2;

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

        public class Button : SettingsButton
        {
            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Height = 100;
                SpriteText.Colour = overlayColourProvider.Background6;
                SpriteText.Font = OsuFont.TorusAlternate.With(size: 34);
            }
        }
    }
}
