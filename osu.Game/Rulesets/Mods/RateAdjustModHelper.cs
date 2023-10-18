// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public class RateAdjustModHelper : IApplicableToTrack
    {
        private readonly BindableBool? adjustPitch;
        private IAdjustableAudioComponent? track;

        public RateAdjustModHelper(BindableNumber<double> speedChange, BindableBool? adjustPitch)
        {
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
    }
}
