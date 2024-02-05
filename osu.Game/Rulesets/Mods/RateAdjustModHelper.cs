// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Provides common functionality shared across various rate adjust mods.
    /// </summary>
    public class RateAdjustModHelper : IApplicableToTrack
    {
        public readonly IBindableNumber<double> SpeedChange;

        private BindableNumber<double> speedChange = new BindableNumber<double>();

        public BindableBool DisableSpeedChange = new BindableBool();

        private IAdjustableAudioComponent? track;

        private BindableBool? adjustPitch;

        /// <summary>
        /// The score multiplier for the current <see cref="SpeedChange"/>.
        /// </summary>
        public double ScoreMultiplier
        {
            get
            {
                // Round to the nearest multiple of 0.1.
                double value = (int)(SpeedChange.Value * 10) / 10.0;

                // Offset back to 0.
                value -= 1;

                if (SpeedChange.Value >= 1)
                    return 1 + value / 5;
                else
                    return 0.6 + value;
            }
        }

        /// <summary>
        /// Construct a new <see cref="RateAdjustModHelper"/>.
        /// </summary>
        /// <param name="speedChange">The main speed adjust parameter which is exposed to the user.</param>
        public RateAdjustModHelper(IBindableNumber<double> speedChange)
        {
            SpeedChange = speedChange;

            SpeedChange.BindValueChanged(_ => updateSpeedChange(), true);
            DisableSpeedChange.BindValueChanged(_ => updateSpeedChange(), true);
        }

        private void updateSpeedChange()
        {
            speedChange.Value = DisableSpeedChange.Value ? 1 : SpeedChange.Value;
        }

        /// <summary>
        /// Setup audio track adjustments for a rate adjust mod.
        /// Importantly, <see cref="ApplyToTrack"/> must be called when a track is obtained/changed for this to work.
        /// </summary>
        /// <param name="adjustPitch">The "adjust pitch" setting as exposed to the user.</param>
        public void HandleAudioAdjustments(BindableBool adjustPitch)
        {
            this.adjustPitch = adjustPitch;

            // When switching between pitch adjust, we need to update adjustments to time-shift or frequency-scale.
            adjustPitch.BindValueChanged(adjustPitchSetting =>
            {
                track?.RemoveAdjustment(adjustmentForPitchSetting(adjustPitchSetting.OldValue), speedChange);
                track?.AddAdjustment(adjustmentForPitchSetting(adjustPitchSetting.NewValue), speedChange);

                AdjustableProperty adjustmentForPitchSetting(bool adjustPitchSettingValue)
                    => adjustPitchSettingValue ? AdjustableProperty.Frequency : AdjustableProperty.Tempo;
            });
        }

        /// <summary>
        /// Should be invoked when a track is obtained / changed.
        /// </summary>
        /// <param name="track">The new track.</param>
        /// <exception cref="InvalidOperationException">If this method is called before <see cref="HandleAudioAdjustments"/>.</exception>
        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            if (adjustPitch == null)
                throw new InvalidOperationException($"Must call {nameof(HandleAudioAdjustments)} first");

            this.track = track;
            adjustPitch.TriggerChange();
        }
    }
}
