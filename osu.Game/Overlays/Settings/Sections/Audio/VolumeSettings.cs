// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using static osu.Game.Audio.DecibelScaling;

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
                    Volume = audio.Volume,
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MasterVolumeInactive,
                    Volume = config.GetBindable<double>(OsuSetting.VolumeInactive),
                    PlaySamplesOnAdjust = true,
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.EffectVolume,
                    Volume = audio.VolumeSample,
                },
                new VolumeAdjustSlider
                {
                    LabelText = AudioSettingsStrings.MusicVolume,
                    Volume = audio.VolumeTrack,
                },
            };
        }

        private partial class VolumeAdjustSlider : SettingsSlider<double>
        {
            protected override Drawable CreateControl() => new DecibelSliderBar();

            public bool PlaySamplesOnAdjust { set => ((DecibelSliderBar)Control).PlaySamplesOnAdjust = value; }

            public Bindable<double> Volume { set => ((DecibelSliderBar)Control).Volume = value; }

            protected partial class DecibelSliderBar : RoundedSliderBar<double>
            {
                public override LocalisableString TooltipText => Current.Value <= DB_MIN ? "-∞ dB" : $"{Current.Value:+#0.0;-#0.0;+0.0} dB";

                public DecibelSliderBar()
                {
                    RelativeSizeAxes = Axes.X;
                    PlaySamplesOnAdjust = false;
                    KeyboardStep = (float)DB_PRECISION;

                    Current = new BindableNumber<double>(0)
                    {
                        Precision = DB_PRECISION,
                        MinValue = DB_MIN,
                        MaxValue = DB_MAX,
                    };
                }

                public Bindable<double> Volume = new Bindable<double>(1);

                private bool currentFirstInvoked;
                private bool volumeFirstInvoked;

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    Current.Default = DecibelFromLinear(Volume.Default);

                    Current.ValueChanged += v =>
                    {
                        if (!volumeFirstInvoked)
                        {
                            currentFirstInvoked = true;
                            Volume.Value = LinearFromDecibel(v.NewValue);
                            currentFirstInvoked = false;
                        }
                    };

                    Volume.BindValueChanged(v =>
                    {
                        if (!currentFirstInvoked)
                        {
                            volumeFirstInvoked = true;
                            Current.Value = DecibelFromLinear(v.NewValue);
                            volumeFirstInvoked = false;
                        }
                    }, true);
                }
            }
        }
    }
}
