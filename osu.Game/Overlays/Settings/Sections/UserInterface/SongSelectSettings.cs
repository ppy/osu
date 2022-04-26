// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public class SongSelectSettings : SettingsSubsection
    {
        private Bindable<double> minStars;
        private Bindable<double> maxStars;

        protected override LocalisableString Header => UserInterfaceStrings.SongSelectHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            minStars = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum);
            maxStars = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum);

            minStars.ValueChanged += min => maxStars.Value = Math.Max(min.NewValue, maxStars.Value);
            maxStars.ValueChanged += max => minStars.Value = Math.Min(max.NewValue, minStars.Value);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    ClassicDefault = true,
                    LabelText = UserInterfaceStrings.RightMouseScroll,
                    Current = config.GetBindable<bool>(OsuSetting.SongSelectRightMouseScroll),
                },
                new SettingsCheckbox
                {
                    LabelText = UserInterfaceStrings.ShowConvertedBeatmaps,
                    Current = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps),
                },
                new SettingsSlider<double, StarsSlider>
                {
                    LabelText = UserInterfaceStrings.StarsMinimum,
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMinimum),
                    KeyboardStep = 0.1f,
                    Keywords = new[] { "minimum", "maximum", "star", "difficulty" }
                },
                new SettingsSlider<double, MaximumStarsSlider>
                {
                    LabelText = UserInterfaceStrings.StarsMaximum,
                    Current = config.GetBindable<double>(OsuSetting.DisplayStarsMaximum),
                    KeyboardStep = 0.1f,
                    Keywords = new[] { "minimum", "maximum", "star", "difficulty" }
                },
                new SettingsEnumDropdown<RandomSelectAlgorithm>
                {
                    LabelText = UserInterfaceStrings.RandomSelectionAlgorithm,
                    Current = config.GetBindable<RandomSelectAlgorithm>(OsuSetting.RandomSelectAlgorithm),
                }
            };
        }

        private class MaximumStarsSlider : StarsSlider
        {
            public override LocalisableString TooltipText => Current.IsDefault ? UserInterfaceStrings.NoLimit : base.TooltipText;
        }

        private class StarsSlider : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Value.ToString(@"0.## stars");
        }
    }
}
