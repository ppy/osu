// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

        private FillFlowContainer letterboxSettings;

        //private Bindable<bool> letterboxing;
        private Bindable<Size> sizeFullscreen;

        private OsuGameBase game;
        private SettingsDropdown<Size> resolutionDropdown;
        private SettingsEnumDropdown<WindowMode> windowModeDropdown;

        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuGameBase game)
        {
            this.game = game;

            //letterboxing = config.GetBindable<bool>(FrameworkSetting.Letterboxing);
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
                    //Bindable = letterboxing,
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
                            //Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionX),
                            KeyboardStep = 0.01f
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "Vertical position",
                            //Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionY),
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

            /*letterboxing.BindValueChanged(isVisible =>
            {
                letterboxSettings.ClearTransforms();
                letterboxSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    letterboxSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            }, true);*/
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
