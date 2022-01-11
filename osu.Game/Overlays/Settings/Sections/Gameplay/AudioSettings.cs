// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class AudioSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GameplaySettingsStrings.AudioHeader;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, OsuConfigManager osuConfig)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = AudioSettingsStrings.PositionalLevel,
                    Keywords = new[] { @"positional", @"balance" },
                    Current = osuConfig.GetBindable<float>(OsuSetting.PositionalHitsoundsLevel),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = GameplaySettingsStrings.AlwaysPlayFirstComboBreak,
                    Current = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak)
                }
            };
        }
    }
}
