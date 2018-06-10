// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

        private FillFlowContainer letterboxSettings;

        private Bindable<bool> letterboxing;
        private Bindable<Size> sizeFullscreen;

        private OsuGame game;
        private SettingsDropdown<int> resolutionDropdown;
        private SettingsEnumDropdown<WindowMode> windowModeDropdown;


        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuGame game)
        {
            this.game = game;

            letterboxing = config.GetBindable<bool>(FrameworkSetting.Letterboxing);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);

            var resolutions = getResolutions();
            var resolutionDropdownBindable = new BindableInt(resolutions.FirstOrDefault(r => r.Key.StartsWith($"{sizeFullscreen.Value.Width}x{sizeFullscreen.Value.Height}")).Value);

            resolutionDropdownBindable.ValueChanged += _ =>
            {
                var newResolution = resolutions.First(r => r.Value == _);
                var newResolutionparts = newResolution.Key.Split('x');
                sizeFullscreen.Value = new Size(int.Parse(newResolutionparts.First()), int.Parse(newResolutionparts.Last()));
            };

            Children = new Drawable[]
            {
                windowModeDropdown = new SettingsEnumDropdown<WindowMode>
                {
                    LabelText = "Screen mode",
                    Bindable = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                resolutionDropdown = new SettingsDropdown<int>
                {
                    LabelText = "Resolution",
                    Items = resolutions,
                    Bindable = resolutionDropdownBindable
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
                            KeyboardStep = 0.1f
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "Vertical position",
                            Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionY),
                            KeyboardStep = 0.1f
                        },
                    }
                },
            };

            windowModeDropdown.Bindable.ValueChanged += (s) =>
            {
                if (windowModeDropdown.Bindable.Value == WindowMode.Fullscreen)
                    resolutionDropdown.Show();
                else
                    resolutionDropdown.Hide();
            };
            windowModeDropdown.Bindable.TriggerChange();

            letterboxing.ValueChanged += isVisible =>
            {
                letterboxSettings.ClearTransforms();
                letterboxSettings.AutoSizeAxes = isVisible ? Axes.Y : Axes.None;

                if (!isVisible)
                    letterboxSettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);
            };
            letterboxing.TriggerChange();
        }

        private List<KeyValuePair<string, int>> getResolutions()
        {
            var availableDisplayResolutions = (game.Window as DesktopGameWindow)?.AvailableDisplayResolutions;
            if (availableDisplayResolutions == null)
                return new List<KeyValuePair<string, int>>();
            var availableDisplayResolutionsStr = availableDisplayResolutions.Select(r => $"{r.Width}x{r.Height}").Distinct().ToList();

            return availableDisplayResolutionsStr.Select((t, i) => new KeyValuePair<string, int>(t, i)).ToList();
        }
    }
}
