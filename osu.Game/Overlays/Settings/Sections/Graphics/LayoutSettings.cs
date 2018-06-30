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
        private readonly BindableInt resolutionDropdownBindable = new BindableInt();

        private OsuGameBase game;
        private SettingsDropdown<int> resolutionDropdown;
        private SettingsEnumDropdown<WindowMode> windowModeDropdown;


        private const int transition_duration = 400;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuGameBase game)
        {
            this.game = game;

            letterboxing = config.GetBindable<bool>(FrameworkSetting.Letterboxing);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);

            sizeFullscreen.ValueChanged += size =>
            {
                KeyValuePair<string, int> valuePair = getResolutions().FirstOrDefault(r => r.Key.StartsWith($"{size.Width}x{size.Height}"));

                resolutionDropdownBindable.Value = valuePair.Value;
            };

            resolutionDropdownBindable.ValueChanged += resolution =>
            {
                var newSelection = getResolutions().First(r => r.Value == resolution);
                var newSelectionParts = newSelection.Key.Split('x');

                var newSelectionWidth = int.Parse(newSelectionParts.First());
                var newSelectionHeight = int.Parse(newSelectionParts.Last());

                if (sizeFullscreen.Value.Width != newSelectionWidth || sizeFullscreen.Value.Height != newSelectionHeight)
                    sizeFullscreen.Value = new Size(newSelectionWidth, newSelectionHeight);
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
                    Items = getResolutions(),
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

            windowModeDropdown.Bindable.ValueChanged += windowMode =>
            {
                if (windowMode == WindowMode.Fullscreen)
                {
                    resolutionDropdown.Show();
                    sizeFullscreen.TriggerChange();
                }
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
            var availableDisplayResolutions = (game.Window as DesktopGameWindow)?.AvailableDisplayResolutions
                                                                                .Where(r => r.Width >= 800 && r.Height >= 600)
                                                                                .OrderByDescending(r => r.Width).ThenByDescending(r => r.Height);

            if (availableDisplayResolutions == null)
                return new List<KeyValuePair<string, int>>();

            var availableDisplayResolutionsStr = availableDisplayResolutions.Select(r => $"{r.Width}x{r.Height}").Distinct();
            return availableDisplayResolutionsStr.Select((t, i) => new KeyValuePair<string, int>(t, i)).ToList();
        }
    }
}
