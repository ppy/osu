// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModTimeRamp : Mod, IUpdatableByPlayfield, IApplicableToBeatmap, IApplicableToRate
    {
        /// <summary>
        /// The point in the beatmap at which the final ramping rate should be reached.
        /// </summary>
        public const double FINAL_RATE_PROGRESS = 0.75f;

        public override double ScoreMultiplier => 0.5;

        [SettingSource("Initial rate", "The starting speed of the track", SettingControlType = typeof(MultiplierSettingsSlider))]
        public abstract BindableNumber<double> InitialRate { get; }

        [SettingSource("Final rate", "The final speed to ramp to", SettingControlType = typeof(MultiplierSettingsSlider))]
        public abstract BindableNumber<double> FinalRate { get; }

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public abstract BindableBool AdjustPitch { get; }

        public override bool ValidForMultiplayerAsFreeMod => false;

        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust), typeof(ModAdaptiveSpeed) };

        public override string SettingDescription => $"{InitialRate.Value:N2}x to {FinalRate.Value:N2}x";

        private double finalRateTime;
        private double beginRampTime;

        public BindableNumber<double> SpeedChange { get; } = new BindableDouble(1)
        {
            Precision = 0.01,
        };

        private readonly RateAdjustModHelper rateAdjustHelper;

        protected ModTimeRamp()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);
            rateAdjustHelper.HandleAudioAdjustments(AdjustPitch);

            // for preview purpose at song select. eventually we'll want to be able to update every frame.
            FinalRate.BindValueChanged(_ => applyRateAdjustment(double.PositiveInfinity), true);
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            rateAdjustHelper.ApplyToTrack(track);
            FinalRate.TriggerChange();
        }

        public void ApplyToSample(IAdjustableAudioComponent sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public virtual void ApplyToBeatmap(IBeatmap beatmap)
        {
            SpeedChange.SetDefault();

            double firstObjectStart = beatmap.HitObjects.FirstOrDefault()?.StartTime ?? 0;
            double lastObjectEnd = beatmap.HitObjects.Any() ? beatmap.GetLastObjectTime() : 0;

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
            applyRateAdjustment(playfield.Clock.CurrentTime);
        }

        /// <summary>
        /// Adjust the rate along the specified ramp.
        /// </summary>
        private void applyRateAdjustment(double time) => SpeedChange.Value = ApplyToRate(time);
    }
}
