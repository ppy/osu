// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateAdjust : Mod, IApplicableToRate, ICanBeToggledDuringReplay
    {
        public override bool ValidForMultiplayerAsFreeMod => false;

        public abstract BindableNumber<double> SpeedChange { get; }

        private readonly Bindable<bool> isDisabled = new Bindable<bool>();
        public bool IsDisable => isDisabled.Value;

        public virtual void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Tempo, SpeedChange);
        }

        public virtual void ApplyToSample(IAdjustableAudioComponent sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public double ApplyToRate(double time, double rate) => rate * SpeedChange.Value;

        private double speedChange;

        public void OnToggle()
        {
            if (isDisabled.Value)
            {
                SpeedChange.Value = speedChange;
                isDisabled.Value = false;
            }
            else
            {
                speedChange = SpeedChange.Value;
                SpeedChange.Value = 1.0;
                isDisabled.Value = true;
            }
        }

        public override Type[] IncompatibleMods => new[] { typeof(ModTimeRamp), typeof(ModAdaptiveSpeed), typeof(ModRateAdjust) };

        public override string SettingDescription => SpeedChange.IsDefault ? string.Empty : $"{SpeedChange.Value:N2}x";
    }
}
