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

        private readonly VolumeScaler volumeInactive = new VolumeScaler();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            config.BindWith(OsuSetting.VolumeInactive, volumeInactive.Real);
            volumeInactive.Scale();

            Children = new Drawable[]
            {
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MasterVolume,
                    Current = audio.Volume.Scaled,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<double>
                {
                    LabelText = AudioSettingsStrings.MasterVolumeInactive,
                    Current = volumeInactive.Scaled,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.EffectVolume,
                    Current = audio.VolumeSample.Scaled,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },

                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MusicVolume,
                    Current = audio.VolumeTrack.Scaled,
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
