// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens
{
    public class LatencyComparerScreen : OsuScreen
    {
        private FrameSync previousFrameSyncMode;
        private double previousActiveHz;

        private readonly OsuTextFlowContainer statusText;

        public override bool HideOverlaysOnEnter => true;

        public override bool CursorVisible => false;

        public override float BackgroundParallaxAmount => 0;

        private readonly OsuTextFlowContainer explanatoryText;

        private readonly Container mainArea;

        private readonly Container resultsArea;

        /// <summary>
        /// The rate at which the game host should attempt to run.
        /// </summary>
        private const int target_host_update_frames = 4000;

        [Cached]
        private readonly OverlayColourProvider overlayColourProvider = new OverlayColourProvider(OverlayColourScheme.Orange);

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private FrameworkConfigManager config { get; set; } = null!;

        private const int rounds_to_complete = 5;

        private int round;
        private int correctCount;
        private int targetRoundCount = rounds_to_complete;

        private int difficulty = 1;

        [Resolved]
        private GameHost host { get; set; } = null!;

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
                explanatoryText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    TextAnchor = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Text = @"Welcome to the latency comparer!
Use the arrow keys or Z/X to move the square.
You can click the targets but you don't have to.
Do whatever you need to try and perceive the difference in latency, then choose your best side.
",
                },
                statusText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 40))
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    TextAnchor = Anchor.TopCentre,
                    Y = 200,
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
            previousActiveHz = host.UpdateThread.ActiveHz;
            config.SetValue(FrameworkSetting.FrameSync, FrameSync.Unlimited);
            host.UpdateThread.ActiveHz = target_host_update_frames;
            // host.AllowBenchmarkUnlimitedFrames = true;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            // host.AllowBenchmarkUnlimitedFrames = false;
            config.SetValue(FrameworkSetting.FrameSync, previousFrameSyncMode);
            host.UpdateThread.ActiveHz = previousActiveHz;
            return base.OnExiting(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            loadNextRound();
        }

        private void recordResult(bool correct)
        {
            explanatoryText.FadeOut(500, Easing.OutQuint);

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
            statusText.Text = $"Difficulty {difficulty}\nRound {round} of {targetRoundCount}";

            mainArea.Clear();

            const int induced_latency = 500;

            int betterSide = RNG.Next(0, 2);

            mainArea.Add(new LatencyArea(Key.Number1, betterSide == 1 ? induced_latency / difficulty : 0)
            {
                Width = 0.5f,
                ReportBetter = () => recordResult(betterSide == 0)
            });

            mainArea.Add(new LatencyArea(Key.Number2, betterSide == 0 ? induced_latency / difficulty : 0)
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

            var displayMode = host.Window.CurrentDisplayMode.Value;

            string exclusive = "unknown";

            if (host.Window is WindowsWindow windowsWindow)
                exclusive = windowsWindow.FullscreenCapability.ToString();

            statusText.Clear();

            float successRate = (float)correctCount / targetRoundCount;
            bool isPass = successRate > 0.8f;

            statusText.AddParagraph($"You scored {correctCount} out of {targetRoundCount} ({successRate:P0})!", cp => cp.Colour = isPass ? colours.Green : colours.Red);

            statusText.AddParagraph($"Level {difficulty} (comparing {host.UpdateThread.Clock.FramesPerSecond:N0}hz and {mapDifficultyToTargetFrameRate(difficulty):N0}hz)",
                cp => cp.Font = OsuFont.Default.With(size: 15));

            statusText.AddParagraph($"Refresh rate: {displayMode.RefreshRate:N0} ExclusiveFullscren: {exclusive}", cp => cp.Font = OsuFont.Default.With(size: 15));

            string cannotIncreaseReason = string.Empty;

            if (!isPass)
                cannotIncreaseReason = "You didn't score high enough (over 80% required)!";
            else if (mapDifficultyToTargetFrameRate(difficulty + 1) > target_host_update_frames)
                cannotIncreaseReason = "You've reached the limits of this comparison mode.";
            else if (mapDifficultyToTargetFrameRate(difficulty + 1) < host.UpdateThread.ActiveHz)
                cannotIncreaseReason = "Game is not running fast enough to test this level";

            resultsArea.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Y = 100,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    new Button(Key.R)
                    {
                        Text = "Increase confidence at current level",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        TooltipText = "The longer you chain, the more sure you will be!",
                        Action = () =>
                        {
                            resultsArea.Clear();
                            targetRoundCount += rounds_to_complete;
                            loadNextRound();
                        }
                    },
                    new Button(Key.I)
                    {
                        Text = "Increase difficulty",
                        BackgroundColour = colours.Red2,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => changeDifficulty(difficulty + 1),
                        Enabled = { Value = string.IsNullOrEmpty(cannotIncreaseReason) },
                        TooltipText = cannotIncreaseReason
                    },
                    new Button(Key.D)
                    {
                        Text = difficulty == 1 ? "Restart" : "Decrease difficulty",
                        BackgroundColour = colours.Green,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => changeDifficulty(Math.Max(difficulty - 1, 1)),
                    }
                }
            });
        }

        private void changeDifficulty(int diff)
        {
            Debug.Assert(diff > 0);

            resultsArea.Clear();

            correctCount = 0;
            round = 0;

            targetRoundCount = rounds_to_complete;
            difficulty = diff;
            loadNextRound();
        }

        private static int mapDifficultyToTargetFrameRate(int difficulty)
        {
            switch (difficulty)
            {
                case 1:
                    return 15;

                case 2:
                    return 30;

                case 3:
                    return 45;

                case 4:
                    return 60;

                case 5:
                    return 120;

                case 6:
                    return 240;

                case 7:
                    return 480;

                case 8:
                    return 720;

                case 9:
                    return 960;

                default:
                    return 1000 + ((difficulty - 10) * 500);
            }
        }

        public class LatencyArea : CompositeDrawable
        {
            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            public Action? ReportBetter { get; set; }

            private Drawable? background;

            private readonly Key key;
            private readonly int inducedLatency;

            private long frameCount;

            public LatencyArea(Key key, int inducedLatency)
            {
                this.key = key;
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
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
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
                    new Button(key)
                    {
                        Text = "Feels better",
                        Y = 20,
                        Width = 0.8f,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Action = () => ReportBetter?.Invoke(),
                    },
                };
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

                        var buttons = inputManager.CurrentState.Keyboard.Keys;

                        box.Colour = buttons.HasAnyButtonPressed ? overlayColourProvider.Content1 : overlayColourProvider.Colour1;

                        foreach (var key in buttons)
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
            private readonly Key key;

            public Button(Key key)
            {
                this.key = key;
            }

            public override LocalisableString Text
            {
                get => base.Text;
                set => base.Text = $"{value} (Press {key.ToString().Replace("Number", string.Empty)})";
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (!e.Repeat && e.Key == key)
                {
                    TriggerClick();
                    return true;
                }

                return base.OnKeyDown(e);
            }

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
