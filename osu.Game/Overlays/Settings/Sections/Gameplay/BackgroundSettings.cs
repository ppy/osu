// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class BackgroundSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.BackgroundHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = GameplaySettingsStrings.BackgroundDim,
                    Current = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = GameplaySettingsStrings.BackgroundBlur,
                    Current = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.LightenDuringBreaks,
                    Current = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks)
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.FadePlayfieldWhenHealthLow,
                    Current = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow),
                },
            };
        }
    }
}
