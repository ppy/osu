// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class FPSCounter : VisibilityContainer, IHasCustomTooltip
    {
        private OsuSpriteText counterUpdateFrameTime = null!;
        private OsuSpriteText counterDrawFPS = null!;

        private Container mainContent = null!;

        private Container background = null!;

        private Container counters = null!;

        private const double min_time_between_updates = 10;

        private const double spike_time_ms = 20;

        private const float idle_background_alpha = 0.4f;

        private readonly BindableBool showFpsDisplay = new BindableBool(true);

        private double displayedFpsCount;
        private double displayedFrameTime;

        private bool isDisplayed;

        private double aimDrawFPS;
        private double aimUpdateFPS;

        private double lastUpdate;
        private ThrottledFrameClock drawClock = null!;
        private ThrottledFrameClock updateClock = null!;
        private ThrottledFrameClock inputClock = null!;

        /// <summary>
        /// The last time value where the display was required (due to a significant change or hovering).
        /// </summary>
        private double lastDisplayRequiredTime;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public FPSCounter()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, GameHost gameHost)
        {
            InternalChildren = new Drawable[]
            {
                mainContent = new Container
                {
                    Alpha = 0,
                    Height = 26,
                    Children = new Drawable[]
                    {
                        background = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = 5,
                            CornerExponent = 5f,
                            Masking = true,
                            Alpha = idle_background_alpha,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colours.Gray0,
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        },
                        counters = new Container
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                counterUpdateFrameTime = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding(1),
                                    Font = OsuFont.Default.With(fixedWidth: true, size: 16, weight: FontWeight.SemiBold),
                                    Spacing = new Vector2(-1),
                                    Y = -2,
                                },
                                counterDrawFPS = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Margin = new MarginPadding(2),
                                    Font = OsuFont.Default.With(fixedWidth: true, size: 13, weight: FontWeight.SemiBold),
                                    Spacing = new Vector2(-2),
                                    Y = 10,
                                }
                            }
                        },
                    }
                },
            };

            config.BindWith(OsuSetting.ShowFpsDisplay, showFpsDisplay);

            drawClock = gameHost.DrawThread.Clock;
            updateClock = gameHost.UpdateThread.Clock;
            inputClock = gameHost.InputThread.Clock;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            requestDisplay();

            showFpsDisplay.BindValueChanged(showFps =>
            {
                State.Value = showFps.NewValue ? Visibility.Visible : Visibility.Hidden;
                if (showFps.NewValue)
                    requestDisplay();
            }, true);

            State.BindValueChanged(state => showFpsDisplay.Value = state.NewValue == Visibility.Visible);
        }

        protected override void PopIn() => this.FadeIn(100);

        protected override void PopOut() => this.FadeOut(100);

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(1, 200);
            requestDisplay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(idle_background_alpha, 200);
            requestDisplay();
            base.OnHoverLost(e);
        }

        protected override void Update()
        {
            base.Update();

            double elapsedDrawFrameTime = drawClock.ElapsedFrameTime;
            double elapsedUpdateFrameTime = updateClock.ElapsedFrameTime;

            // If the game goes into a suspended state (ie. debugger attached or backgrounded on a mobile device)
            // we want to ignore really long periods of no processing.
            if (elapsedUpdateFrameTime > 10000)
                return;

            mainContent.Width = Math.Max(mainContent.Width, counters.DrawWidth);

            // Handle the case where the window has become inactive or the user changed the
            // frame limiter (we want to show the FPS as it's changing, even if it isn't an outlier).
            bool aimRatesChanged = updateAimFPS();

            bool hasUpdateSpike = displayedFrameTime < spike_time_ms && elapsedUpdateFrameTime > spike_time_ms;
            // use elapsed frame time rather then FramesPerSecond to better catch stutter frames.
            bool hasDrawSpike = displayedFpsCount > (1000 / spike_time_ms) && elapsedDrawFrameTime > spike_time_ms;

            const float damp_time = 100;

            displayedFrameTime = Interpolation.DampContinuously(displayedFrameTime, elapsedUpdateFrameTime, hasUpdateSpike ? 0 : damp_time, elapsedUpdateFrameTime);

            if (hasDrawSpike)
                // show spike time using raw elapsed value, to account for `FramesPerSecond` being so averaged spike frames don't show.
                displayedFpsCount = 1000 / elapsedDrawFrameTime;
            else
                displayedFpsCount = Interpolation.DampContinuously(displayedFpsCount, drawClock.FramesPerSecond, damp_time, Time.Elapsed);

            if (Time.Current - lastUpdate > min_time_between_updates)
            {
                updateFpsDisplay();
                updateFrameTimeDisplay();

                lastUpdate = Time.Current;
            }

            bool hasSignificantChanges = aimRatesChanged
                                         || hasDrawSpike
                                         || hasUpdateSpike
                                         || displayedFpsCount < aimDrawFPS * 0.8
                                         || 1000 / displayedFrameTime < aimUpdateFPS * 0.8;

            if (hasSignificantChanges)
                requestDisplay();
            else if (isDisplayed && Time.Current - lastDisplayRequiredTime > 2000 && !IsHovered)
            {
                mainContent.FadeTo(0.7f, 300, Easing.OutQuint);
                isDisplayed = false;
            }
        }

        private void requestDisplay()
        {
            lastDisplayRequiredTime = Time.Current;

            if (!isDisplayed)
            {
                mainContent.FadeTo(1, 300, Easing.OutQuint);
                isDisplayed = true;
            }
        }

        private void updateFpsDisplay()
        {
            counterDrawFPS.Colour = getColour(displayedFpsCount / aimDrawFPS);
            counterDrawFPS.Text = $"{displayedFpsCount:#,0}fps";
        }

        private void updateFrameTimeDisplay()
        {
            counterUpdateFrameTime.Text = displayedFrameTime < 5
                ? $"{displayedFrameTime:N1}ms"
                : $"{displayedFrameTime:N0}ms";

            counterUpdateFrameTime.Colour = getColour((1000 / displayedFrameTime) / aimUpdateFPS);
        }

        private bool updateAimFPS()
        {
            if (updateClock.Throttling)
            {
                double newAimDrawFPS = drawClock.MaximumUpdateHz;
                double newAimUpdateFPS = updateClock.MaximumUpdateHz;

                if (aimDrawFPS != newAimDrawFPS || aimUpdateFPS != newAimUpdateFPS)
                {
                    aimDrawFPS = newAimDrawFPS;
                    aimUpdateFPS = newAimUpdateFPS;
                    return true;
                }
            }
            else
            {
                double newAimFPS = inputClock.MaximumUpdateHz;

                if (aimDrawFPS != newAimFPS || aimUpdateFPS != newAimFPS)
                {
                    aimUpdateFPS = aimDrawFPS = newAimFPS;
                    return true;
                }
            }

            return false;
        }

        private ColourInfo getColour(double performanceRatio)
        {
            if (performanceRatio < 0.5f)
                return Interpolation.ValueAt(performanceRatio, colours.Red, colours.Orange2, 0, 0.5);

            return Interpolation.ValueAt(performanceRatio, colours.Orange2, colours.Lime0, 0.5, 0.9);
        }

        public ITooltip GetCustomTooltip() => new FPSCounterTooltip();

        public object TooltipContent => this;
    }
}
