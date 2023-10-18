// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Provides common functionality shared across various rate adjust mods.
    /// </summary>
    public class RateAdjustModHelper : IApplicableToTrack
    {
        private readonly BindableBool? adjustPitch;
        private readonly BindableNumber<double> speedChange;
        private IAdjustableAudioComponent? track;

        public RateAdjustModHelper(BindableNumber<double> speedChange, BindableBool? adjustPitch = null)
        {
            this.speedChange = speedChange;
            this.adjustPitch = adjustPitch;

            // When switching between pitch adjust, we need to update adjustments to time-shift or frequency-scale.
            adjustPitch?.BindValueChanged(adjustPitchSetting =>
            {
                track?.RemoveAdjustment(adjustmentForPitchSetting(adjustPitchSetting.OldValue), speedChange);
                track?.AddAdjustment(adjustmentForPitchSetting(adjustPitchSetting.NewValue), speedChange);

                AdjustableProperty adjustmentForPitchSetting(bool adjustPitchSettingValue)
                    => adjustPitchSettingValue ? AdjustableProperty.Frequency : AdjustableProperty.Tempo;
            });
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            this.track = track;
            adjustPitch?.TriggerChange();
        }

        public double ScoreMultiplier
        {
            get
            {
                // Round to the nearest multiple of 0.1.
                double value = (int)(speedChange.Value * 10) / 10.0;

                // Offset back to 0.
                value -= 1;

                if (speedChange.Value >= 1)
                    value /= 5;

                return 1 + value;
            }
        }
    }
}
