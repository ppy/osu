// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

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
                    LabelText = "Screen mode",
                    ItemSource = windowModes,
                    Current = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                resolutionDropdown = new ResolutionSettingsDropdown
                {
                    LabelText = "Resolution",
                    ShowsDefaultIndicator = false,
                    ItemSource = resolutions,
                    Current = sizeFullscreen
                },
                new SettingsSlider<float, UIScaleSlider>
                {
                    LabelText = "UI Scaling",
                    TransferValueOnCommit = true,
                    Current = osuConfig.GetBindable<float>(OsuSetting.UIScale),
                    KeyboardStep = 0.01f,
                    Keywords = new[] { "scale", "letterbox" },
                },
                new SettingsEnumDropdown<ScalingMode>
                {
                    LabelText = "Screen Scaling",
                    Current = osuConfig.GetBindable<ScalingMode>(OsuSetting.Scaling),
                    Keywords = new[] { "scale", "letterbox" },
                },
                scalingSettings = new FillFlowContainer<SettingsSlider<float>>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = transition_duration,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,
                    Children = new[]
                    {
                        new SettingsSlider<float>
                        {
                            LabelText = "Horizontal position",
                            Current = scalingPositionX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "Vertical position",
                            Current = scalingPositionY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "Horizontal scale",
                            Current = scalingSizeX,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                        new SettingsSlider<float>
                        {
                            LabelText = "Vertical scale",
                            Current = scalingSizeY,
                            KeyboardStep = 0.01f,
                            DisplayAsPercentage = true
                        },
                    }
                },
            };

            windowModes.BindCollectionChanged((sender, args) =>
            {
                if (windowModes.Count > 1)
                    windowModeDropdown.Show();
                else
                    windowModeDropdown.Hide();
            }, true);

            windowModeDropdown.Current.ValueChanged += _ => updateResolutionDropdown();

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

            scalingSettings.ForEach(s => bindPreviewEvent(s.Current));

            scalingMode.BindValueChanged(mode =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeAxes = mode.NewValue != ScalingMode.Off ? Axes.Y : Axes.None;

                if (mode.NewValue == ScalingMode.Off)
                    scalingSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);

                scalingSettings.ForEach(s => s.TransferValueOnCommit = mode.NewValue == ScalingMode.Everything);
            }, true);

            void updateResolutionDropdown()
            {
                if (resolutions.Count > 1 && windowModeDropdown.Current.Value == WindowMode.Fullscreen)
                    resolutionDropdown.Show();
                else
                    resolutionDropdown.Hide();
            }
        }

        /// <summary>
        /// Create a delayed bindable which only updates when a condition is met.
        /// </summary>
        /// <param name="bindable">The config bindable.</param>
        /// <returns>A bindable which will propagate updates with a delay.</returns>
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
            public override string TooltipText => base.TooltipText + "x";
        }

        private class ResolutionSettingsDropdown : SettingsDropdown<Size>
        {
            protected override OsuDropdown<Size> CreateDropdown() => new ResolutionDropdownControl();

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
