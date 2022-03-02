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
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModAdaptiveSpeed : Mod, IApplicableToRate, IApplicableToDrawableHitObject, IApplicableToBeatmap
    {
        // use a wider range so there's still room for adjustment when the initial rate is extreme
        private const double fastest_rate = 2.5f;
        private const double slowest_rate = 0.4f;

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
            Precision = 0.01
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
            Precision = 0.01
        };

        private ITrack track;

        private readonly List<double> recentRates = Enumerable.Range(0, average_count).Select(_ => 1d).ToList();

        // rate for a hit is calculated using the end time of another hit object earlier in time
        // caching them here for easy access
        private readonly Dictionary<HitObject, double> previousEndTimes = new Dictionary<HitObject, double>();

        // record the value removed from recentRates when an object is hit
        // for rewind support
        private readonly Dictionary<HitObject, double> dequeuedRates = new Dictionary<HitObject, double>();

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

        public double ApplyToRate(double time, double rate = 1) => rate * InitialRate.Value;

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
                {
                    dequeuedRates.Add(result.HitObject, recentRates[0]);
                    recentRates.RemoveAt(0);
                }

                SpeedChange.Value = recentRates.Average();
            };
            drawable.OnRevertResult += (o, result) =>
            {
                if (!result.IsHit) return;
                if (!previousEndTimes.ContainsKey(result.HitObject)) return;

                if (dequeuedRates.ContainsKey(result.HitObject))
                {
                    recentRates.Insert(0, dequeuedRates[result.HitObject]);
                    recentRates.RemoveAt(recentRates.Count - 1);
                    dequeuedRates.Remove(result.HitObject);
                }
                else
                {
                    recentRates.Insert(0, InitialRate.Value);
                    recentRates.RemoveAt(recentRates.Count - 1);
                }

                SpeedChange.Value = recentRates.Average();
            };
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var endTimes = getEndTimes(beatmap.HitObjects).OrderBy(x => x).ToList();

            for (int i = 1; i < beatmap.HitObjects.Count; i++)
            {
                var hitObject = beatmap.HitObjects[i];
                double prevEndTime = endTimes.LastOrDefault(ht => !Precision.AlmostBigger(ht, hitObject.GetEndTime()));

                if (prevEndTime != default)
                    previousEndTimes.Add(hitObject, prevEndTime);
            }
        }

        private IEnumerable<double> getEndTimes(IEnumerable<HitObject> hitObjects)
        {
            foreach (var hitObject in hitObjects)
            {
                if (!(hitObject.HitWindows is HitWindows.EmptyHitWindows))
                    yield return hitObject.GetEndTime();

                foreach (double hitTime in getEndTimes(hitObject.NestedHitObjects))
                    yield return hitTime;
            }
        }
    }
}
