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
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public class FPSCounter : VisibilityContainer, IHasCustomTooltip
    {
        private OsuSpriteText counterUpdateFrameTime = null!;
        private OsuSpriteText counterDrawFPS = null!;

        private Container mainContent = null!;

        private Container background = null!;

        private Container counters = null!;

        private const float idle_background_alpha = 0.4f;

        private readonly BindableBool showFpsDisplay = new BindableBool(true);

        private double displayedFpsCount;
        private double displayedFrameTime;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public FPSCounter()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayTemporarily();

            showFpsDisplay.BindValueChanged(showFps =>
            {
                State.Value = showFps.NewValue ? Visibility.Visible : Visibility.Hidden;
                if (showFps.NewValue)
                    displayTemporarily();
            }, true);

            State.BindValueChanged(state => showFpsDisplay.Value = state.NewValue == Visibility.Visible);
        }

        protected override void PopIn() => this.FadeIn(100);

        protected override void PopOut() => this.FadeOut(100);

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(1, 200);
            displayTemporarily();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(idle_background_alpha, 200);
            displayTemporarily();
            base.OnHoverLost(e);
        }

        private bool isDisplayed;

        private ScheduledDelegate? fadeOutDelegate;

        private double aimDrawFPS;
        private double aimUpdateFPS;

        private void displayTemporarily()
        {
            if (!isDisplayed)
            {
                mainContent.FadeTo(1, 300, Easing.OutQuint);
                isDisplayed = true;
            }

            fadeOutDelegate?.Cancel();
            fadeOutDelegate = null;

            if (!IsHovered)
            {
                fadeOutDelegate = Scheduler.AddDelayed(() =>
                {
                    mainContent.FadeTo(0, 300, Easing.OutQuint);
                    isDisplayed = false;
                }, 2000);
            }
        }

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        protected override void Update()
        {
            base.Update();

            mainContent.Width = Math.Max(mainContent.Width, counters.DrawWidth);

            // Handle the case where the window has become inactive or the user changed the
            // frame limiter (we want to show the FPS as it's changing, even if it isn't an outlier).
            bool aimRatesChanged = updateAimFPS();

            // TODO: this is wrong (elapsed clock time, not actual run time).
            double newUpdateFrameTime = gameHost.UpdateThread.Clock.ElapsedFrameTime;
            double newDrawFrameTime = gameHost.DrawThread.Clock.ElapsedFrameTime;
            double newDrawFps = gameHost.DrawThread.Clock.FramesPerSecond;

            const double spike_time_ms = 20;

            bool hasUpdateSpike = displayedFrameTime < spike_time_ms && newUpdateFrameTime > spike_time_ms;
            // use elapsed frame time rather then FramesPerSecond to better catch stutter frames.
            bool hasDrawSpike = displayedFpsCount > (1000 / spike_time_ms) && newDrawFrameTime > spike_time_ms;

            // note that we use an elapsed time here of 1 intentionally.
            // this weights all updates equally. if we passed in the elapsed time, longer frames would be weighted incorrectly lower.
            displayedFrameTime = Interpolation.DampContinuously(displayedFrameTime, newUpdateFrameTime, hasUpdateSpike ? 0 : 100, 1);

            if (hasDrawSpike)
                // show spike time using raw elapsed value, to account for `FramesPerSecond` being so averaged spike frames don't show.
                displayedFpsCount = 1000 / newDrawFrameTime;
            else
                displayedFpsCount = Interpolation.DampContinuously(displayedFpsCount, newDrawFps, 100, Time.Elapsed);

            updateFpsDisplay();
            updateFrameTimeDisplay();

            bool hasSignificantChanges = aimRatesChanged
                                         || hasDrawSpike
                                         || hasUpdateSpike
                                         || displayedFpsCount < aimDrawFPS * 0.8
                                         || 1000 / displayedFrameTime < aimUpdateFPS * 0.8;

            if (hasSignificantChanges)
                displayTemporarily();
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
            if (gameHost.UpdateThread.Clock.Throttling)
            {
                double newAimDrawFPS = gameHost.DrawThread.Clock.MaximumUpdateHz;
                double newAimUpdateFPS = gameHost.UpdateThread.Clock.MaximumUpdateHz;

                if (aimDrawFPS != newAimDrawFPS || aimUpdateFPS != newAimUpdateFPS)
                {
                    aimDrawFPS = newAimDrawFPS;
                    aimUpdateFPS = newAimUpdateFPS;
                    return true;
                }
            }
            else
            {
                double newAimFPS = gameHost.InputThread.Clock.MaximumUpdateHz;

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
