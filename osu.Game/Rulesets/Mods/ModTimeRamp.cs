// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeRamp : Mod, IUpdatableByPlayfield, IApplicableToBeatmap, IApplicableToTrack
    {
        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        private const double final_rate_progress = 0.75f;

        [SettingSource("Initial rate", "The starting speed of the track")]
        public abstract BindableNumber<double> InitialRate { get; }

        [SettingSource("Final rate", "The final speed to ramp to")]
        public abstract BindableNumber<double> FinalRate { get; }

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public abstract BindableBool AdjustPitch { get; }

        public override string SettingDescription => $"{InitialRate.Value:N2}x to {FinalRate.Value:N2}x";

        private double finalRateTime;
        private double beginRampTime;

        public BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        private Track track;

        protected ModTimeRamp()
        {
            // for preview purpose at song select. eventually we'll want to be able to update every frame.
            FinalRate.BindValueChanged(val => applyRateAdjustment(1), true);
            AdjustPitch.BindValueChanged(applyPitchAdjustment);
        }

        public void ApplyToTrack(Track track)
        {
            this.track = track;

            FinalRate.TriggerChange();
            AdjustPitch.TriggerChange();
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            HitObject lastObject = beatmap.HitObjects.LastOrDefault();

            SpeedChange.SetDefault();

            beginRampTime = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            finalRateTime = final_rate_progress * (lastObject?.GetEndTime() ?? 0);
        }

        public virtual void Update(Playfield playfield)
        {
            applyRateAdjustment((track.CurrentTime - beginRampTime) / finalRateTime);
        }

        /// <summary>
        /// Adjust the rate along the specified ramp
        /// </summary>
        /// <param name="amount">The amount of adjustment to apply (from 0..1).</param>
        private void applyRateAdjustment(double amount) =>
            SpeedChange.Value = InitialRate.Value + (FinalRate.Value - InitialRate.Value) * Math.Clamp(amount, 0, 1);

        private void applyPitchAdjustment(ValueChangedEvent<bool> adjustPitchSetting)
        {
            // remove existing old adjustment
            track.RemoveAdjustment(adjustmentForPitchSetting(adjustPitchSetting.OldValue), SpeedChange);

            track.AddAdjustment(adjustmentForPitchSetting(adjustPitchSetting.NewValue), SpeedChange);
        }

        private AdjustableProperty adjustmentForPitchSetting(bool adjustPitchSettingValue)
            => adjustPitchSettingValue ? AdjustableProperty.Frequency : AdjustableProperty.Tempo;
    }
}
