// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Audio;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRateAdjust : Mod, IApplicableToRate
    {
        public abstract BindableNumber<double> SpeedChange { get; }

        public abstract BindableBool AdjustPitch { get; }

        protected ITrack Track;

        public virtual void ApplyToTrack(ITrack track)
        {
            Track = track;

            AdjustPitch.TriggerChange();
        }

        public virtual void ApplyToSample(DrawableSample sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public double ApplyToRate(double time, double rate) => rate * SpeedChange.Value;

        public override string SettingDescription => SpeedChange.IsDefault ? string.Empty : $"{SpeedChange.Value:N2}x";

        protected ModRateAdjust()
        {
            AdjustPitch.BindValueChanged(applyPitchAdjustment);
        }

        private void applyPitchAdjustment(ValueChangedEvent<bool> adjustPitchSetting)
        {
            Track?.RemoveAdjustment(adjustmentForPitchSetting(adjustPitchSetting.OldValue), SpeedChange);
            Track?.AddAdjustment(adjustmentForPitchSetting(adjustPitchSetting.NewValue), SpeedChange);
        }

        private AdjustableProperty adjustmentForPitchSetting(bool adjustPitchSettingValue)
            => adjustPitchSettingValue ? AdjustableProperty.Frequency : AdjustableProperty.Tempo;
    }
}
