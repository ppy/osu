// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

        private FillFlowContainer scalingSettings;

        private Bindable<ScalingMode> scalingMode;
        private Bindable<Size> sizeFullscreen;

        private OsuGameBase game;
        private SettingsDropdown<Size> resolutionDropdown;
        private SettingsEnumDropdown<WindowMode> windowModeDropdown;

        private Bindable<float> scalingPositionX;
        private Bindable<float> scalingPositionY;
        private Bindable<float> scalingSizeX;
        private Bindable<float> scalingSizeY;

        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, OsuGameBase game)
        {
            this.game = game;

            scalingMode = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);
            scalingSizeX = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingPositionX = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionX);
            scalingPositionY = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionY);

            Container resolutionSettingsContainer;

            Children = new Drawable[]
            {
                windowModeDropdown = new SettingsEnumDropdown<WindowMode>
                {
                    LabelText = "Screen mode",
                    Bindable = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                resolutionSettingsContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                new SettingsEnumDropdown<ScalingMode>
                {
                    LabelText = "Scaling",
                    Bindable = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling),
                },
                scalingSettings = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = transition_duration,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new SettingsSlider<float>
                        {
                            LabelText = "Horizontal position",
                            Bindable = delayedBindable(scalingPositionX),
                            KeyboardStep = 0.01f
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "Vertical position",
                            Bindable = delayedBindable(scalingPositionY),
                            KeyboardStep = 0.01f
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "Horizontal scale",
                            Bindable = delayedBindable(scalingSizeX),
                            KeyboardStep = 0.01f
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "Vertical scale",
                            Bindable = delayedBindable(scalingSizeY),
                            KeyboardStep = 0.01f
                        },
                    }
                },
            };

            var resolutions = getResolutions();

            if (resolutions.Count > 1)
            {
                resolutionSettingsContainer.Child = resolutionDropdown = new ResolutionSettingsDropdown
                {
                    LabelText = "Resolution",
                    ShowsDefaultIndicator = false,
                    Items = resolutions,
                    Bindable = sizeFullscreen
                };

                windowModeDropdown.Bindable.BindValueChanged(windowMode =>
                {
                    if (windowMode == WindowMode.Fullscreen)
                    {
                        resolutionDropdown.Show();
                        sizeFullscreen.TriggerChange();
                    }
                    else
                        resolutionDropdown.Hide();
                }, true);
            }

            scalingMode.BindValueChanged(mode =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeAxes = mode != ScalingMode.Off ? Axes.Y : Axes.None;

                if (mode == ScalingMode.Off)
                    scalingSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            }, true);
        }

        /// <summary>
        /// Create a delayed bindable which only updates when a condition is met.
        /// </summary>
        /// <param name="configBindable">The config bindable.</param>
        /// <returns>A bindable which will propagate updates with a delay.</returns>
        private Bindable<float> delayedBindable(Bindable<float> configBindable)
        {
            var delayed = new BindableFloat { MinValue = 0, MaxValue = 1, Default = configBindable.Default };

            configBindable.BindValueChanged(v => delayed.Value = v, true);
            delayed.ValueChanged += v =>
            {
                switch (scalingMode.Value)
                {
                    case ScalingMode.Everything:
                        applyWithDelay(() => configBindable.Value = v);
                        return;
                    case ScalingMode.Gameplay:
                        showPreview();
                        break;
                }

                configBindable.Value = v;
            };

            return delayed;
        }

        private Drawable preview;
        private void showPreview()
        {
            if (preview?.IsAlive != true)
                game.Add(preview = new ScalingPreview());

            preview.FadeOutFromOne(1500);
            preview.Expire();
        }

        private ScheduledDelegate delayedApplication;

        private void applyWithDelay(Action func, bool firstRun = true)
        {
            if (!firstRun && !GetContainingInputManager().CurrentState.Mouse.IsPressed(MouseButton.Left))
            {
                func();
                return;
            }

            delayedApplication?.Cancel();
            delayedApplication = Scheduler.AddDelayed(() => applyWithDelay(func, false), 250);
        }

        private IReadOnlyList<Size> getResolutions()
        {
            var resolutions = new List<Size> { new Size(9999, 9999) };

            if (game.Window != null)
            {
                resolutions.AddRange(game.Window.AvailableResolutions
                                         .Where(r => r.Width >= 800 && r.Height >= 600)
                                         .OrderByDescending(r => r.Width)
                                         .ThenByDescending(r => r.Height)
                                         .Select(res => new Size(res.Width, res.Height))
                                         .Distinct());
            }

            return resolutions;
        }

        private class ScalingPreview : ScalingContainer
        {
            public ScalingPreview()
            {
                Child = new Box
                {
                    Colour = Color4.White,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                };
            }
        }

        private class ResolutionSettingsDropdown : SettingsDropdown<Size>
        {
            protected override OsuDropdown<Size> CreateDropdown() => new ResolutionDropdownControl { Items = Items };

            private class ResolutionDropdownControl : DropdownControl
            {
                protected override string GenerateItemText(Size item)
                {
                    if (item == new Size(9999, 9999))
                        return "Default";
                    return $"{item.Width}x{item.Height}";
                }
            }
        }
    }
}
