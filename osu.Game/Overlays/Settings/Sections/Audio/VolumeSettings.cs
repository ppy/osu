// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public partial class VolumeSettings : SettingsSubsection
    {
        protected override LocalisableString Header => AudioSettingsStrings.VolumeHeader;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MasterVolume,
                    Current = audio.Volume,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = AudioSettingsStrings.MasterVolumeInactive,
                    Current = config.GetBindable<double>(OsuSetting.VolumeInactive),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.EffectVolume,
                    Current = audio.VolumeSample,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },

                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MusicVolume,
                    Current = audio.VolumeTrack,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
            };
        }

        private partial class VolumeAdjustSlider : SettingsSlider<double>
        {
            protected override Drawable CreateControl()
            {
                var sliderBar = (RoundedSliderBar<double>)base.CreateControl();
                sliderBar.PlaySamplesOnAdjust = false;
                return sliderBar;
            }
        }
    }
}
