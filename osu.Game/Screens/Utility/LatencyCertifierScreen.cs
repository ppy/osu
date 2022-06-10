// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility
{
    public class LatencyCertifierScreen : OsuScreen
    {
        private FrameSync previousFrameSyncMode;
        private double previousActiveHz;

        private readonly OsuTextFlowContainer statusText;

        public override bool HideOverlaysOnEnter => true;

        public override bool CursorVisible => mainArea.Count == 0;

        public override float BackgroundParallaxAmount => 0;

        private readonly OsuTextFlowContainer explanatoryText;

        private readonly Container<LatencyArea> mainArea;

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

        private const int rounds_to_complete_certified = 20;

        private int attemptsAtCurrentDifficulty;
        private int correctAtCurrentDifficulty;
        private int totalRoundForNextResultsScreen = rounds_to_complete;

        private int difficultyLevel = 1;

        private double lastPoll;
        private int pollingMax;

        [Resolved]
        private GameHost host { get; set; } = null!;

        public LatencyCertifierScreen()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = overlayColourProvider.Background6,
                    RelativeSizeAxes = Axes.Both,
                },
                mainArea = new Container<LatencyArea>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                // Make sure the edge between the two comparisons can't be used to ascertain latency.
                new Box
                {
                    Name = "separator",
                    Colour = ColourInfo.GradientHorizontal(overlayColourProvider.Background6, overlayColourProvider.Background6.Opacity(0)),
                    Width = 100,
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft,
                },
                new Box
                {
                    Name = "separator",
                    Colour = ColourInfo.GradientHorizontal(overlayColourProvider.Background6.Opacity(0), overlayColourProvider.Background6),
                    Width = 100,
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
                    Text = @"Welcome to the latency certifier!
Use the arrow keys, Z/X/J/K to move the square.
Use the Tab key to change focus.
Do whatever you need to try and perceive the difference in latency, then choose your best side.
",
                },
                resultsArea = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                statusText = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 40))
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    TextAnchor = Anchor.TopCentre,
                    Y = 150,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            };
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (lastPoll > 0)
                pollingMax = (int)Math.Max(pollingMax, 1000 / (Clock.CurrentTime - lastPoll));
            lastPoll = Clock.CurrentTime;
            return base.OnMouseMove(e);
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            previousFrameSyncMode = config.Get<FrameSync>(FrameworkSetting.FrameSync);
            previousActiveHz = host.UpdateThread.ActiveHz;
            config.SetValue(FrameworkSetting.FrameSync, FrameSync.Unlimited);
            host.UpdateThread.ActiveHz = target_host_update_frames;
            host.AllowBenchmarkUnlimitedFrames = true;
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            host.AllowBenchmarkUnlimitedFrames = false;
            config.SetValue(FrameworkSetting.FrameSync, previousFrameSyncMode);
            host.UpdateThread.ActiveHz = previousActiveHz;
            return base.OnExiting(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            loadNextRound();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Tab:
                    var firstArea = mainArea.FirstOrDefault(a => !a.IsActiveArea.Value);
                    if (firstArea != null)
                        firstArea.IsActiveArea.Value = true;
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private void showResults()
        {
            mainArea.Clear();

            var displayMode = host.Window?.CurrentDisplayMode.Value;

            string exclusive = "unknown";

            if (host.Window is WindowsWindow windowsWindow)
                exclusive = windowsWindow.FullscreenCapability.ToString();

            statusText.Clear();

            float successRate = (float)correctAtCurrentDifficulty / totalRoundForNextResultsScreen;
            bool isPass = successRate == 1;

            statusText.AddParagraph($"You scored {correctAtCurrentDifficulty} out of {totalRoundForNextResultsScreen} ({successRate:0%})!", cp => cp.Colour = isPass ? colours.Green : colours.Red);
            statusText.AddParagraph($"Level {difficultyLevel} ({mapDifficultyToTargetFrameRate(difficultyLevel):N0} hz)",
                cp => cp.Font = OsuFont.Default.With(size: 24));

            statusText.AddParagraph(string.Empty);
            statusText.AddParagraph(string.Empty);
            statusText.AddIcon(isPass ? FontAwesome.Regular.CheckCircle : FontAwesome.Regular.TimesCircle, cp => cp.Colour = isPass ? colours.Green : colours.Red);
            statusText.AddParagraph(string.Empty);

            if (!isPass && difficultyLevel > 1)
            {
                statusText.AddParagraph("To complete certification, decrease the difficulty level until you can get 20 tests correct in a row!",
                    cp => cp.Font = OsuFont.Default.With(size: 24, weight: FontWeight.SemiBold));
                statusText.AddParagraph(string.Empty);
            }

            statusText.AddParagraph($"Polling: {pollingMax} hz Monitor: {displayMode?.RefreshRate ?? 0:N0} hz Exclusive: {exclusive}",
                cp => cp.Font = OsuFont.Default.With(size: 15, weight: FontWeight.SemiBold));

            statusText.AddParagraph($"Input: {host.InputThread.Clock.FramesPerSecond} hz "
                                    + $"Update: {host.UpdateThread.Clock.FramesPerSecond} hz "
                                    + $"Draw: {host.DrawThread.Clock.FramesPerSecond} hz"
                , cp => cp.Font = OsuFont.Default.With(size: 15, weight: FontWeight.SemiBold));

            int certificationRemaining = !isPass ? rounds_to_complete_certified : rounds_to_complete_certified - correctAtCurrentDifficulty;

            if (isPass && certificationRemaining <= 0)
            {
                Drawable background;
                Drawable certifiedText;

                resultsArea.AddRange(new[]
                {
                    background = new Box
                    {
                        Colour = overlayColourProvider.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    (certifiedText = new OsuSpriteText
                    {
                        Alpha = 0,
                        Font = OsuFont.TorusAlternate.With(size: 80, weight: FontWeight.Bold),
                        Text = "Certified!",
                        Blending = BlendingParameters.Additive,
                    }).WithEffect(new GlowEffect
                    {
                        Colour = overlayColourProvider.Colour1,
                        PadExtent = true
                    }).With(e =>
                    {
                        e.Anchor = Anchor.Centre;
                        e.Origin = Anchor.Centre;
                    }),
                    new OsuSpriteText
                    {
                        Text = $"You should use a frame limiter with update rate of {mapDifficultyToTargetFrameRate(difficultyLevel + 1)} hz (or fps) for best results!",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Torus.With(size: 24, weight: FontWeight.SemiBold),
                        Y = 80,
                    }
                });

                background.FadeInFromZero(1000, Easing.OutQuint);

                certifiedText.FadeInFromZero(500, Easing.InQuint);

                certifiedText
                    .ScaleTo(10)
                    .ScaleTo(1, 600, Easing.InQuad)
                    .Then()
                    .ScaleTo(1.05f, 10000, Easing.OutQuint);
                return;
            }

            string cannotIncreaseReason = string.Empty;

            if (!isPass)
                cannotIncreaseReason = "You didn't get a perfect score.";
            else if (mapDifficultyToTargetFrameRate(difficultyLevel + 1) > target_host_update_frames)
                cannotIncreaseReason = "You've reached the maximum level.";
            else if (mapDifficultyToTargetFrameRate(difficultyLevel + 1) > Clock.FramesPerSecond)
                cannotIncreaseReason = "Game is not running fast enough to test this level";

            resultsArea.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Spacing = new Vector2(20),
                Padding = new MarginPadding(20),
                Children = new Drawable[]
                {
                    new ButtonWithKeyBind(Key.Enter)
                    {
                        Text = "Continue to next level",
                        BackgroundColour = colours.Red2,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => changeDifficulty(difficultyLevel + 1),
                        Enabled = { Value = string.IsNullOrEmpty(cannotIncreaseReason) },
                        TooltipText = cannotIncreaseReason
                    },
                    new ButtonWithKeyBind(Key.D)
                    {
                        Text = difficultyLevel == 1 ? "Retry" : "Return to last level",
                        BackgroundColour = colours.Green,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () => changeDifficulty(Math.Max(difficultyLevel - 1, 1)),
                    },
                    new ButtonWithKeyBind(Key.C)
                    {
                        Text = $"Continue towards certification at this level ({certificationRemaining} more)",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = () =>
                        {
                            resultsArea.Clear();
                            totalRoundForNextResultsScreen += rounds_to_complete;
                            loadNextRound();
                        },
                        TooltipText = isPass ? $"Chain {rounds_to_complete_certified} to confirm your perception!" : "You've reached your limits. Go to the previous level to complete certification!",
                        Enabled = { Value = isPass },
                    },
                }
            });
        }

        private void changeDifficulty(int difficulty)
        {
            Debug.Assert(difficulty > 0);

            resultsArea.Clear();

            correctAtCurrentDifficulty = 0;
            attemptsAtCurrentDifficulty = 0;

            pollingMax = 0;
            lastPoll = 0;

            totalRoundForNextResultsScreen = rounds_to_complete;
            difficultyLevel = difficulty;

            loadNextRound();
        }

        private void loadNextRound()
        {
            attemptsAtCurrentDifficulty++;
            statusText.Text = $"Level {difficultyLevel}\nRound {attemptsAtCurrentDifficulty} of {totalRoundForNextResultsScreen}";

            mainArea.Clear();

            int betterSide = RNG.Next(0, 2);

            mainArea.AddRange(new[]
            {
                new LatencyArea(Key.Number1, betterSide == 1 ? mapDifficultyToTargetFrameRate(difficultyLevel) : (int?)null)
                {
                    Width = 0.5f,
                    IsActiveArea = { Value = true },
                    ReportUserBest = () => recordResult(betterSide == 0),
                },
                new LatencyArea(Key.Number2, betterSide == 0 ? mapDifficultyToTargetFrameRate(difficultyLevel) : (int?)null)
                {
                    Width = 0.5f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    ReportUserBest = () => recordResult(betterSide == 1)
                }
            });

            foreach (var area in mainArea)
            {
                area.IsActiveArea.BindValueChanged(active =>
                {
                    if (active.NewValue)
                        mainArea.Children.First(a => a != area).IsActiveArea.Value = false;
                });
            }
        }

        private void recordResult(bool correct)
        {
            // Fading this out will improve the frame rate after the first round due to less text on screen.
            explanatoryText.FadeOut(500, Easing.OutQuint);

            if (correct)
                correctAtCurrentDifficulty++;

            if (attemptsAtCurrentDifficulty < totalRoundForNextResultsScreen)
                loadNextRound();
            else
                showResults();
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
    }
}
