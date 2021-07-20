// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class VolumeSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "音量";

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "整体",
                    Current = audio.Volume,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "整体（失去焦点时）",
                    Current = config.GetBindable<double>(OsuSetting.VolumeInactive),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "音效",
                    Current = audio.VolumeSample,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = "音乐",
                    Current = audio.VolumeTrack,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
            };
        }
    }
}
