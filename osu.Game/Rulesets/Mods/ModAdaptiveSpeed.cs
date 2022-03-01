// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Audio;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mods
{
    public class ModAdaptiveSpeed : Mod, IApplicableToRate, IApplicableToDrawableHitObject, IApplicableToBeatmap
    {
        private const double fastest_rate = 2f;

        private const double slowest_rate = 0.5f;

        /// <summary>
        /// Adjust track rate using the average speed of the last x hits
        /// </summary>
        private const int average_count = 10;

        public override string Name => "Adaptive Speed";

        public override string Acronym => "AS";

        public override string Description => "Let track speed adapt to you.";

        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust), typeof(ModTimeRamp) };

        [SettingSource("Initial rate", "The starting speed of the track")]
        public BindableNumber<double> InitialRate { get; } = new BindableDouble
        {
            MinValue = 0.5,
            MaxValue = 2,
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public BindableBool AdjustPitch { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        private ITrack track;

        private readonly List<double> recentRates = Enumerable.Range(0, average_count).Select(_ => 1d).ToList();

        // rates are calculated using the end time of the previous hit object
        // caching them here for easy access
        private readonly Dictionary<HitObject, double> previousEndTimes = new Dictionary<HitObject, double>();

        public ModAdaptiveSpeed()
        {
            InitialRate.BindValueChanged(val => SpeedChange.Value = val.NewValue);
            AdjustPitch.BindValueChanged(applyPitchAdjustment);
        }

        public void ApplyToTrack(ITrack track)
        {
            this.track = track;

            InitialRate.TriggerChange();
            AdjustPitch.TriggerChange();
            recentRates.Clear();
            recentRates.AddRange(Enumerable.Range(0, average_count).Select(_ => InitialRate.Value));
        }

        public void ApplyToSample(DrawableSample sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public double ApplyToRate(double time, double rate = 1) => rate;

        private void applyPitchAdjustment(ValueChangedEvent<bool> adjustPitchSetting)
        {
            // remove existing old adjustment
            track?.RemoveAdjustment(adjustmentForPitchSetting(adjustPitchSetting.OldValue), SpeedChange);

            track?.AddAdjustment(adjustmentForPitchSetting(adjustPitchSetting.NewValue), SpeedChange);
        }

        private AdjustableProperty adjustmentForPitchSetting(bool adjustPitchSettingValue)
            => adjustPitchSettingValue ? AdjustableProperty.Frequency : AdjustableProperty.Tempo;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            drawable.OnNewResult += (o, result) =>
            {
                if (!result.IsHit) return;
                if (!previousEndTimes.ContainsKey(result.HitObject)) return;

                double prevEndTime = previousEndTimes[result.HitObject];

                recentRates.Add(Math.Clamp((result.HitObject.GetEndTime() - prevEndTime) / (result.TimeAbsolute - prevEndTime) * SpeedChange.Value, slowest_rate, fastest_rate));
                if (recentRates.Count > average_count)
                    recentRates.RemoveAt(0);

                SpeedChange.Value = recentRates.Average();
            };
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var hitObject = beatmap.HitObjects[i];
                var previousObject = beatmap.HitObjects.Take(i).LastOrDefault(o => !Precision.AlmostBigger(o.GetEndTime(), hitObject.GetEndTime()));

                if (previousObject != null)
                    previousEndTimes.Add(hitObject, previousObject.GetEndTime());
            }
        }
    }
}
