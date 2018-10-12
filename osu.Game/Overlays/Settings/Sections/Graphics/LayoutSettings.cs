// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

        private FillFlowContainer letterboxSettings;

        private Bindable<bool> letterboxing;
        private Bindable<Size> sizeFullscreen;

        private OsuGameBase game;
        private SettingsDropdown<DisplayResolutionItem> resolutionDropdown;
        private SettingsEnumDropdown<WindowMode> windowModeDropdown;

        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuGameBase game)
        {
            this.game = game;

            letterboxing = config.GetBindable<bool>(FrameworkSetting.Letterboxing);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);

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
                new SettingsCheckbox
                {
                    LabelText = "Letterboxing",
                    Bindable = letterboxing,
                },
                letterboxSettings = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = transition_duration,
                    AutoSizeEasing = Easing.OutQuint,
                    Masking = true,

                    Children = new Drawable[]
                    {
                        new SettingsSlider<double>
                        {
                            LabelText = "Horizontal position",
                            Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionX),
                            KeyboardStep = 0.01f
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "Vertical position",
                            Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionY),
                            KeyboardStep = 0.01f
                        },
                    }
                },
            };

            var resolutions = getResolutions();

            if (resolutions.Count > 1)
            {
                resolutionSettingsContainer.Child = resolutionDropdown = new SettingsDropdown<DisplayResolutionItem>
                {
                    LabelText = "Fullscreen Resolution",
                    ShowsDefaultIndicator = false,
                    Items = resolutions
                };

                // TODO do something with the selected refresh rate and bpp settings
                // Todo: Create helpers to link two bindables with converters?
                resolutionDropdown.Bindable = new Bindable<DisplayResolutionItem>();
                resolutionDropdown.Bindable.BindValueChanged(item => sizeFullscreen.Value = item.Size);
                sizeFullscreen.BindValueChanged(size => resolutionDropdown.Bindable.Value = resolutionDropdown.Items.FirstOrDefault(item => item.Size == size));

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

            letterboxing.BindValueChanged(isVisible =>
            {
                letterboxSettings.ClearTransforms();
                letterboxSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    letterboxSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            }, true);
        }

        private IReadOnlyList<DisplayResolutionItem> getResolutions()
        {
            var resolutions = DisplayResolutionItem.DEFAULT.Yield();

            if (game.Window != null)
                resolutions = resolutions.Concat(game.Window.AvailableResolutions
                                                     .Where(r => r.Width >= 800 && r.Height >= 600)
                                                     .OrderByDescending(r => r.Width)
                                                     .ThenByDescending(r => r.Height)
                                                     .Select(res => new Size(res.Width, res.Height))
                                                     .Distinct()
                                                     // TODO: keep multiple identical sizes with different refresh rates
                                                     // https://github.com/ppy/osu-framework/pull/1946#issuecomment-429009249
                                                     .Select(size => new DisplayResolutionItem(size, 0, 0)));
            return resolutions.ToList();
        }

        private class DisplayResolutionItem
        {
            public static readonly DisplayResolutionItem DEFAULT = new DefaultItem();

            public readonly Size Size;
            // TODO for now, these values are completely ignored
            // ReSharper disable NotAccessedField.Local
            public readonly float RefreshRate;
            public readonly int BitsPerPixel;

            public DisplayResolutionItem(Size size, float refreshRate, int bitsPerPixel)
            {
                Size = size;
                RefreshRate = refreshRate;
                BitsPerPixel = bitsPerPixel;
            }

            public override string ToString() => $"{Size.Width}x{Size.Height}"; //$"{Size.Width}x{Size.Height} @{RefreshRate} Hz ({BitsPerPixel} bpp)";

            private class DefaultItem : DisplayResolutionItem
            {
                public DefaultItem()
                    : base(new Size(9999, 9999), 60, 32)
                {
                }

                // TODO make this localisable
                public override string ToString() => "Default";
            }
        }
    }
}
