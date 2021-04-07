// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeRamp : Mod, IUpdatableByPlayfield, IApplicableToBeatmap, IApplicableToRate
    {
        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        public const double FINAL_RATE_PROGRESS = 0.75f;

        [SettingSource("Initial rate", "The starting speed of the track")]
        public abstract BindableNumber<double> InitialRate { get; }

        [SettingSource("Final rate", "The final speed to ramp to")]
        public abstract BindableNumber<double> FinalRate { get; }

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public abstract BindableBool AdjustPitch { get; }

        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust) };

        public override string SettingDescription => $"{InitialRate.Value:N2}x to {FinalRate.Value:N2}x";

        private double finalRateTime;
        private double beginRampTime;

        public BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        private ITrack track;

        protected ModTimeRamp()
        {
            // for preview purpose at song select. eventually we'll want to be able to update every frame.
            FinalRate.BindValueChanged(val => applyRateAdjustment(double.PositiveInfinity), true);
            AdjustPitch.BindValueChanged(applyPitchAdjustment);
        }

        public void ApplyToTrack(ITrack track)
        {
            this.track = track;

            FinalRate.TriggerChange();
            AdjustPitch.TriggerChange();
        }

        public void ApplyToSample(DrawableSample sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            SpeedChange.SetDefault();

            double firstObjectStart = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            double lastObjectEnd = beatmap.HitObjects.LastOrDefault()?.GetEndTime() ?? 0;

            beginRampTime = firstObjectStart;
            finalRateTime = firstObjectStart + FINAL_RATE_PROGRESS * (lastObjectEnd - firstObjectStart);
        }

        public double ApplyToRate(double time, double rate = 1)
        {
            double amount = (time - beginRampTime) / Math.Max(1, finalRateTime - beginRampTime);
            double ramp = InitialRate.Value + (FinalRate.Value - InitialRate.Value) * Math.Clamp(amount, 0, 1);

            // round the end result to match the bindable SpeedChange's precision, in case this is called externally.
            return rate * Math.Round(ramp, 2);
        }

        public virtual void Update(Playfield playfield)
        {
            applyRateAdjustment(track.CurrentTime);
        }

        /// <summary>
        /// Adjust the rate along the specified ramp.
        /// </summary>
        private void applyRateAdjustment(double time) => SpeedChange.Value = ApplyToRate(time);

        private void applyPitchAdjustment(ValueChangedEvent<bool> adjustPitchSetting)
        {
            // remove existing old adjustment
            track?.RemoveAdjustment(adjustmentForPitchSetting(adjustPitchSetting.OldValue), SpeedChange);

            track?.AddAdjustment(adjustmentForPitchSetting(adjustPitchSetting.NewValue), SpeedChange);
        }

        private AdjustableProperty adjustmentForPitchSetting(bool adjustPitchSettingValue)
            => adjustPitchSettingValue ? AdjustableProperty.Frequency : AdjustableProperty.Tempo;
    }
}
