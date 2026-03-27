// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public partial class BackgroundSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.BackgroundHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsItemV2(new FormSliderBar<double>
                {
                    Caption = GameplaySettingsStrings.BackgroundDim,
                    Current = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                }),
                new SettingsItemV2(new FormSliderBar<double>
                {
                    Caption = GameplaySettingsStrings.BackgroundBlur,
                    Current = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GameplaySettingsStrings.LightenDuringBreaks,
                    Current = config.GetBindable<bool>(OsuSetting.LightenDuringBreaks),
                })
                {
                    Keywords = new[] { "dim", "level" }
                },
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = GameplaySettingsStrings.FadePlayfieldWhenHealthLow,
                    Current = config.GetBindable<bool>(OsuSetting.FadePlayfieldWhenHealthLow),
                }),
            };
        }
    }
}
