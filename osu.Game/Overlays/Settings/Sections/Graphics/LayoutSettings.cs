﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.LayoutHeader;

        private FillFlowContainer<SettingsSlider<float>> scalingSettings;

        private readonly IBindable<Display> currentDisplay = new Bindable<Display>();
        private readonly IBindableList<WindowMode> windowModes = new BindableList<WindowMode>();

        private Bindable<ScalingMode> scalingMode;
        private Bindable<Size> sizeFullscreen;

        private readonly BindableList<Size> resolutions = new BindableList<Size>(new[] { new Size(9999, 9999) });

        [Resolved]
        private OsuGameBase game { get; set; }

        private SettingsDropdown<Size> resolutionDropdown;
        private SettingsDropdown<WindowMode> windowModeDropdown;

        private Bindable<float> scalingPositionX;
        private Bindable<float> scalingPositionY;
        private Bindable<float> scalingSizeX;
        private Bindable<float> scalingSizeY;

        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, GameHost host)
        {
            scalingMode = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);
            scalingSizeX = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeY);
            scalingPositionX = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionX);
            scalingPositionY = osuConfig.GetBindable<float>(OsuSetting.ScalingPositionY);

            if (host.Window != null)
            {
                currentDisplay.BindTo(host.Window.CurrentDisplayBindable);
                windowModes.BindTo(host.Window.SupportedWindowModes);
            }

            Children = new Drawable[]
            {
                windowModeDropdown = new SettingsDropdown<WindowMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenMode,
                    ItemSource = windowModes,
                    Current = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                resolutionDropdown = new ResolutionSettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Resolution,
                    ShowsDefaultIndicator = false,
                    ItemSource = resolutions,
                    Current = sizeFullscreen
                },
                new SettingsSlider<float, UIScaleSlider>
                {
                    LabelText = GraphicsSettingsStrings.UIScaling,
                    TransferValueOnCommit = true,
                    Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                    KeyboardStep = 0.01f,
                    Keywords = new[] { "scale", "letterbox" },
                },
                new SettingsEnumDropdown<ScalingMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenScaling,
                    Current = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling),
                    Keywords = new[] { "scale", "letterbox" },
                },
                scalingSettings = new FillFlowContainer<SettingsSlider<float>>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new[]
                    {
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.HorizontalPosition,
                            Current = scalingPositionX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.VerticalPosition,
                            Current = scalingPositionY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.HorizontalScale,
                            Current = scalingSizeX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = GraphicsSettingsStrings.VerticalScale,
                            Current = scalingSizeY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scalingSettings.ForEach(s => bindPreviewEvent(s.Current));

            windowModeDropdown.Current.BindValueChanged(mode =>
            {
                updateResolutionDropdown();

                windowModeDropdown.WarningText = mode.NewValue != WindowMode.Fullscreen ? GraphicsSettingsStrings.NotFullscreenNote : default;
            }, true);

            windowModes.BindCollectionChanged((sender, args) =>
            {
                if (windowModes.Count > 1)
                    windowModeDropdown.Show();
                else
                    windowModeDropdown.Hide();
            }, true);

            currentDisplay.BindValueChanged(display => Schedule(() =>
            {
                resolutions.RemoveRange(1, resolutions.Count - 1);

                if (display.NewValue != null)
                {
                    resolutions.AddRange(display.NewValue.DisplayModes
                                                .Where(m => m.Size.Width >= 800 && m.Size.Height >= 600)
                                                .OrderByDescending(m => Math.Max(m.Size.Height, m.Size.Width))
                                                .Select(m => m.Size)
                                                .Distinct());
                }

                updateResolutionDropdown();
            }), true);

            scalingMode.BindValueChanged(mode =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeDuration = transition_duration;
                scalingSettings.AutoSizeEasing = Easing.OutQuint;

                updateScalingModeVisibility();
            });

            // initial update bypasses transforms
            updateScalingModeVisibility();

            void updateResolutionDropdown()
            {
                if (resolutions.Count > 1 && windowModeDropdown.Current.Value == WindowMode.Fullscreen)
                    resolutionDropdown.Show();
                else
                    resolutionDropdown.Hide();
            }

            void updateScalingModeVisibility()
            {
                if (scalingMode.Value == ScalingMode.Off)
                    scalingSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);

                scalingSettings.AutoSizeAxes = scalingMode.Value != ScalingMode.Off ? Axes.Y : Axes.None;
                scalingSettings.ForEach(s => s.TransferValueOnCommit = scalingMode.Value == ScalingMode.Everything);
            }
        }

        private void bindPreviewEvent(Bindable<float> bindable)
        {
            bindable.ValueChanged += _ =>
            {
                switch (scalingMode.Value)
                {
                    case ScalingMode.Gameplay:
                        showPreview();
                        break;
                }
            };
        }

        private Drawable preview;

        private void showPreview()
        {
            if (preview?.IsAlive != true)
                game.Add(preview = new ScalingPreview());

            preview.FadeOutFromOne(1500);
            preview.Expire();
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

        private class UIScaleSlider : OsuSliderBar<float>
        {
            public override LocalisableString TooltipText => base.TooltipText + "x";
        }

        private class ResolutionSettingsDropdown : SettingsDropdown<Size>
        {
            protected override OsuDropdown<Size> CreateDropdown() => new ResolutionDropdownControl();

            private class ResolutionDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Size item)
                {
                    if (item == new Size(9999, 9999))
                        return CommonStrings.Default;

                    return $"{item.Width}x{item.Height}";
                }
            }
        }
    }
}
