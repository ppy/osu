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
                    KeyboardStep = (float)VolumeScaler.STEP,
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MasterVolumeInactive,
                    Current = volumeInactive.Scaled,
                    KeyboardStep = (float)VolumeScaler.STEP,
                    PlaySamplesOnAdjust = true,
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.EffectVolume,
                    Current = audio.VolumeSample.Scaled,
                    KeyboardStep = (float)VolumeScaler.STEP,
                },

                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MusicVolume,
                    Current = audio.VolumeTrack.Scaled,
                    KeyboardStep = (float)VolumeScaler.STEP,
                },
            };
        }

        private partial class DecibelSliderBar : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => (Current.Value <= VolumeScaler.MIN ? "-∞" : Current.Value.ToString("+#0.0;-#0.0;+0.0")) + " dB";
        }

        private partial class VolumeAdjustSlider : SettingsSlider<double>
        {
            protected override Drawable CreateControl() => new DecibelSliderBar
            {
                RelativeSizeAxes = Axes.X,
                PlaySamplesOnAdjust = false,
            };

            public bool PlaySamplesOnAdjust
            {
                get => ((DecibelSliderBar)Control).PlaySamplesOnAdjust;
                set => ((DecibelSliderBar)Control).PlaySamplesOnAdjust = value;
            }
        }
    }
}
