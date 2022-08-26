// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Drawing;
using System.Linq;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
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

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();
        private readonly IBindableList<WindowMode> windowModes = new BindableList<WindowMode>();

        private Bindable<ScalingMode> scalingMode;
        private Bindable<Size> sizeFullscreen;

        private readonly BindableList<Size> resolutions = new BindableList<Size>(new[] { new Size(9999, 9999) });
        private readonly IBindable<FullscreenCapability> fullscreenCapability = new Bindable<FullscreenCapability>(FullscreenCapability.Capable);

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        private SettingsDropdown<Size> resolutionDropdown;
        private SettingsDropdown<Display> displayDropdown;
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

            if (host.Window is WindowsWindow windowsWindow)
                fullscreenCapability.BindTo(windowsWindow.FullscreenCapability);

            Children = new Drawable[]
            {
                windowModeDropdown = new SettingsDropdown<WindowMode>
                {
                    LabelText = GraphicsSettingsStrings.ScreenMode,
                    ItemSource = windowModes,
                    Current = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                displayDropdown = new DisplaySettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Display,
                    Items = host.Window?.Displays,
                    Current = currentDisplay,
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

            fullscreenCapability.BindValueChanged(_ => Schedule(updateScreenModeWarning), true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scalingSettings.ForEach(s => bindPreviewEvent(s.Current));

            windowModeDropdown.Current.BindValueChanged(_ =>
            {
                updateDisplayModeDropdowns();
                updateScreenModeWarning();
            }, true);

            windowModes.BindCollectionChanged((_, _) =>
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

                updateDisplayModeDropdowns();
            }), true);

            scalingMode.BindValueChanged(_ =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeDuration = transition_duration;
                scalingSettings.AutoSizeEasing = Easing.OutQuint;

                updateScalingModeVisibility();
            });

            // initial update bypasses transforms
            updateScalingModeVisibility();

            void updateDisplayModeDropdowns()
            {
                if (resolutions.Count > 1 && windowModeDropdown.Current.Value == WindowMode.Fullscreen)
                    resolutionDropdown.Show();
                else
                    resolutionDropdown.Hide();

                if (displayDropdown.Items.Count() > 1)
                    displayDropdown.Show();
                else
                    displayDropdown.Hide();
            }

            void updateScalingModeVisibility()
            {
                if (scalingMode.Value == ScalingMode.Off)
                    scalingSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);

                scalingSettings.AutoSizeAxes = scalingMode.Value != ScalingMode.Off ? Axes.Y : Axes.None;
                scalingSettings.ForEach(s => s.TransferValueOnCommit = scalingMode.Value == ScalingMode.Everything);
            }
        }

        private void updateScreenModeWarning()
        {
            if (RuntimeInfo.OS == RuntimeInfo.Platform.macOS)
            {
                if (windowModeDropdown.Current.Value == WindowMode.Fullscreen)
                    windowModeDropdown.SetNoticeText(LayoutSettingsStrings.FullscreenMacOSNote, true);
                else
                    windowModeDropdown.ClearNoticeText();

                return;
            }

            if (windowModeDropdown.Current.Value != WindowMode.Fullscreen)
            {
                windowModeDropdown.SetNoticeText(GraphicsSettingsStrings.NotFullscreenNote, true);
                return;
            }

            if (host.Window is WindowsWindow)
            {
                switch (fullscreenCapability.Value)
                {
                    case FullscreenCapability.Unknown:
                        windowModeDropdown.SetNoticeText(LayoutSettingsStrings.CheckingForFullscreenCapabilities, true);
                        break;

                    case FullscreenCapability.Capable:
                        windowModeDropdown.SetNoticeText(LayoutSettingsStrings.OsuIsRunningExclusiveFullscreen);
                        break;

                    case FullscreenCapability.Incapable:
                        windowModeDropdown.SetNoticeText(LayoutSettingsStrings.UnableToRunExclusiveFullscreen, true);
                        break;
                }
            }
            else
            {
                // We can only detect exclusive fullscreen status on windows currently.
                windowModeDropdown.ClearNoticeText();
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

        private class DisplaySettingsDropdown : SettingsDropdown<Display>
        {
            protected override OsuDropdown<Display> CreateDropdown() => new DisplaySettingsDropdownControl();

            private class DisplaySettingsDropdownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(Display item)
                {
                    return $"{item.Index}: {item.Name} ({item.Bounds.Width}x{item.Bounds.Height})";
                }
            }
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
