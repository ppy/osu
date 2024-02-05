// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    public class ModAdaptiveSpeed : Mod, IApplicableToRate, IApplicableToDrawableHitObject, IApplicableToBeatmap, IUpdatableByPlayfield, ICanBeToggledDuringReplay
    {
        public override string Name => "Adaptive Speed";

        public override string Acronym => "AS";

        public override LocalisableString Description => "Let track speed adapt to you.";

        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 0.5;

        public sealed override bool ValidForMultiplayer => false;
        public sealed override bool ValidForMultiplayerAsFreeMod => false;

        public override Type[] IncompatibleMods => new[] { typeof(ModRateAdjust), typeof(ModTimeRamp), typeof(ModAutoplay) };

        [SettingSource("Initial rate", "The starting speed of the track", SettingControlType = typeof(MultiplierSettingsSlider))]
        public BindableNumber<double> InitialRate { get; } = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 2,
            Precision = 0.01
        };

        [SettingSource("Adjust pitch", "Should pitch be adjusted with speed")]
        public BindableBool AdjustPitch { get; } = new BindableBool(true);

        public BindableBool IsDisabled { get; } = new BindableBool();

        /// <summary>
        /// The instantaneous rate of the track.
        /// Every frame this mod will attempt to smoothly adjust this to meet <see cref="targetRate"/>.
        /// </summary>
        public BindableNumber<double> SpeedChange { get; } = new BindableDouble(1)
        {
            MinValue = min_allowable_rate,
            MaxValue = max_allowable_rate,
        };

        // The two constants below denote the maximum allowable range of rates that `SpeedChange` can take.
        // The range is purposefully wider than the range of values that `InitialRate` allows
        // in order to give some leeway for change even when extreme initial rates are chosen.
        private const double min_allowable_rate = 0.4d;
        private const double max_allowable_rate = 2.5d;

        // The two constants below denote the maximum allowable change in rate caused by a single hit
        // This prevents sudden jolts caused by a badly-timed hit.
        private const double min_allowable_rate_change = 0.9d;
        private const double max_allowable_rate_change = 1.11d;

        // Apply a fixed rate change when missing, allowing the player to catch up when the rate is too fast.
        private const double rate_change_on_miss = 0.95d;

        private double targetRate = 1d;

        /// <summary>
        /// The number of most recent track rates (approximated from how early/late each object was hit relative to the previous object)
        /// which should be averaged to calculate <see cref="targetRate"/>.
        /// </summary>
        private const int recent_rate_count = 8;

        /// <summary>
        /// Stores the most recent <see cref="recent_rate_count"/> approximated track rates
        /// which are averaged to calculate the value of <see cref="targetRate"/>.
        /// </summary>
        /// <remarks>
        /// This list is used as a double-ended queue with fixed capacity
        /// (items can be enqueued/dequeued at either end of the list).
        /// When time is elapsing forward, items are dequeued from the start and enqueued onto the end of the list.
        /// When time is being rewound, items are dequeued from the end and enqueued onto the start of the list.
        /// </remarks>
        /// <example>
        /// <para>
        /// The track rate approximation is calculated as follows:
        /// </para>
        /// <para>
        /// Consider a hitobject which ends at 1000ms, and assume that its preceding hitobject ends at 500ms.
        /// This gives a time difference of 1000 - 500 = 500ms.
        /// </para>
        /// <para>
        /// Now assume that the user hit this object at 980ms rather than 1000ms.
        /// When compared to the preceding hitobject, this gives 980 - 500 = 480ms.
        /// </para>
        /// <para>
        /// With the above assumptions, the player is rushing / hitting early, which means that the track should speed up to match.
        /// Therefore, the approximated target rate for this object would be equal to 500 / 480 * <see cref="InitialRate"/>.
        /// </para>
        /// </example>
        private readonly List<double> recentRates = Enumerable.Repeat(1d, recent_rate_count).ToList();

        /// <summary>
        /// For each given <see cref="HitObject"/> in the map, this dictionary maps the object onto the latest end time of any other object
        /// that precedes the end time of the given object.
        /// This can be loosely interpreted as the end time of the preceding hit object in rulesets that do not have overlapping hit objects.
        /// </summary>
        private readonly Dictionary<HitObject, double> precedingEndTimes = new Dictionary<HitObject, double>();

        /// <summary>
        /// For each given <see cref="HitObject"/> in the map, this dictionary maps the object onto the track rate dequeued from
        /// <see cref="recentRates"/> (i.e. the oldest value in the queue) when the object is hit. If the hit is then reverted,
        /// the mapped value can be re-introduced to <see cref="recentRates"/> to properly rewind the queue.
        /// </summary>
        private readonly Dictionary<HitObject, double> ratesForRewinding = new Dictionary<HitObject, double>();

        private readonly RateAdjustModHelper rateAdjustHelper;

        public ModAdaptiveSpeed()
        {
            rateAdjustHelper = new RateAdjustModHelper(SpeedChange);
            rateAdjustHelper.HandleAudioAdjustments(AdjustPitch);

            InitialRate.BindValueChanged(val =>
            {
                SpeedChange.Value = val.NewValue;
                targetRate = val.NewValue;
            });
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            InitialRate.TriggerChange();
            recentRates.Clear();
            recentRates.AddRange(Enumerable.Repeat(InitialRate.Value, recent_rate_count));

            rateAdjustHelper.ApplyToTrack(track);
        }

        public void ApplyToSample(IAdjustableAudioComponent sample)
        {
            sample.AddAdjustment(AdjustableProperty.Frequency, SpeedChange);
        }

        public void Update(Playfield playfield)
        {
            SpeedChange.Value = IsDisabled.Value ? InitialRate.Value : Interpolation.DampContinuously(SpeedChange.Value, targetRate, 50, playfield.Clock.ElapsedFrameTime);
        }

        public double ApplyToRate(double time, double rate = 1) => rate * InitialRate.Value;

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            drawable.OnNewResult += (_, result) =>
            {
                if (ratesForRewinding.ContainsKey(result.HitObject)) return;
                if (!shouldProcessResult(result)) return;

                ratesForRewinding.Add(result.HitObject, recentRates[0]);
                recentRates.RemoveAt(0);

                recentRates.Add(Math.Clamp(getRelativeRateChange(result) * SpeedChange.Value, min_allowable_rate, max_allowable_rate));

                updateTargetRate();
            };
            drawable.OnRevertResult += (_, result) =>
            {
                if (!ratesForRewinding.ContainsKey(result.HitObject)) return;
                if (!shouldProcessResult(result)) return;

                recentRates.Insert(0, ratesForRewinding[result.HitObject]);
                ratesForRewinding.Remove(result.HitObject);

                recentRates.RemoveAt(recentRates.Count - 1);

                updateTargetRate();
            };
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var hitObjects = getAllApplicableHitObjects(beatmap.HitObjects).ToList();
            var endTimes = hitObjects.Select(x => x.GetEndTime()).OrderBy(x => x).Distinct().ToList();

            foreach (HitObject hitObject in hitObjects)
            {
                int index = endTimes.BinarySearch(hitObject.GetEndTime());
                if (index < 0) index = ~index; // BinarySearch returns the next larger element in bitwise complement if there's no exact match
                index -= 1;

                if (index >= 0)
                    precedingEndTimes.Add(hitObject, endTimes[index]);
            }
        }

        private IEnumerable<HitObject> getAllApplicableHitObjects(IEnumerable<HitObject> hitObjects)
        {
            foreach (var hitObject in hitObjects)
            {
                if (!(hitObject.HitWindows is HitWindows.EmptyHitWindows))
                    yield return hitObject;

                foreach (HitObject nested in getAllApplicableHitObjects(hitObject.NestedHitObjects))
                    yield return nested;
            }
        }

        private bool shouldProcessResult(JudgementResult result)
        {
            if (!result.Type.AffectsAccuracy()) return false;
            if (!precedingEndTimes.ContainsKey(result.HitObject)) return false;

            return true;
        }

        private double getRelativeRateChange(JudgementResult result)
        {
            if (!result.IsHit)
                return rate_change_on_miss;

            double prevEndTime = precedingEndTimes[result.HitObject];
            return Math.Clamp(
                (result.HitObject.GetEndTime() - prevEndTime) / (result.TimeAbsolute - prevEndTime),
                min_allowable_rate_change,
                max_allowable_rate_change
            );
        }

        /// <summary>
        /// Update <see cref="targetRate"/> based on the values in <see cref="recentRates"/>.
        /// </summary>
        private void updateTargetRate()
        {
            // Compare values in recentRates to see how consistent the player's speed is
            // If the player hits half of the notes too fast and the other half too slow:  Abs(consistency) = 0
            // If the player hits all their notes too fast or too slow:                    Abs(consistency) = recent_rate_count - 1
            int consistency = 0;

            for (int i = 1; i < recentRates.Count; i++)
            {
                consistency += Math.Sign(recentRates[i] - recentRates[i - 1]);
            }

            // Scale the rate adjustment based on consistency
            targetRate = Interpolation.Lerp(targetRate, recentRates.Average(), Math.Abs(consistency) / (recent_rate_count - 1d));
        }
    }
}
