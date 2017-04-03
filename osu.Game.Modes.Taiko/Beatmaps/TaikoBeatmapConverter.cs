﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Types;
using osu.Game.Modes.Taiko.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;

namespace osu.Game.Modes.Taiko.Beatmaps
{
    internal class TaikoBeatmapConverter : IBeatmapConverter<TaikoHitObject>
    {
        /// <summary>
        /// osu! is generally slower than taiko, so a factor is added to increase
        /// speed. This must be used everywhere slider length or beat length is used.
        /// </summary>
        private const float legacy_velocity_multiplier = 1.4f;

        /// <summary>
        /// Because swells are easier in taiko than spinners are in osu!,
        /// legacy taiko multiplies a factor when converting the number of required hits.
        /// </summary>
        private const float swell_hit_multiplier = 1.65f;

        /// <summary>
        /// Base osu! slider scoring distance.
        /// </summary>
        private const float osu_base_scoring_distance = 100;

        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float taiko_base_distance = 100;

        public Beatmap<TaikoHitObject> Convert(Beatmap original)
        {
            return new Beatmap<TaikoHitObject>(original)
            {
                HitObjects = original.HitObjects.SelectMany(h => convertHitObject(h, original)).ToList()
            };
        }

        private IEnumerable<TaikoHitObject> convertHitObject(HitObject obj, Beatmap beatmap)
        {
            // Check if this HitObject is already a TaikoHitObject, and return it if so
            var originalTaiko = obj as TaikoHitObject;
            if (originalTaiko != null)
                yield return originalTaiko;

            var distanceData = obj as IHasDistance;
            var repeatsData = obj as IHasRepeats;
            var endTimeData = obj as IHasEndTime;

            // Old osu! used hit sounding to determine various hit type information
            SampleType sample = obj.Sample?.Type ?? SampleType.None;

            bool strong = (sample & SampleType.Finish) > 0;

            if (distanceData != null)
            {
                int repeats = repeatsData?.RepeatCount ?? 1;

                double speedAdjustment = beatmap.TimingInfo.SpeedMultiplierAt(obj.StartTime);
                double speedAdjustedBeatLength = beatmap.TimingInfo.BeatLengthAt(obj.StartTime) * speedAdjustment;
                
                // The true distance, accounting for any repeats. This ends up being the drum roll distance later
                double distance = distanceData.Distance * repeats * legacy_velocity_multiplier;

                // The velocity of the taiko hit object - calculated as the velocity of a drum roll
                double taikoVelocity = taiko_base_distance * beatmap.BeatmapInfo.Difficulty.SliderMultiplier / speedAdjustedBeatLength * legacy_velocity_multiplier;
                // The duration of the taiko hit object
                double taikoDuration = distance / taikoVelocity;

                // For some reason, old osu! always uses speedAdjustment to determine the taiko velocity, but
                // only uses it to determine osu! velocity if beatmap version < 8. Let's account for that here.
                if (beatmap.BeatmapInfo.BeatmapVersion >= 8)
                    speedAdjustedBeatLength /= speedAdjustment;

                // The velocity of the osu! hit object - calculated as the velocity of a slider
                double osuVelocity = osu_base_scoring_distance * beatmap.BeatmapInfo.Difficulty.SliderMultiplier / speedAdjustedBeatLength * legacy_velocity_multiplier;
                // The duration of the osu! hit object
                double osuDuration = distance / osuVelocity;

                // If the drum roll is to be split into hit circles, assume the ticks are 1/8 spaced within the duration of one beat
                double tickSpacing = Math.Min(speedAdjustedBeatLength / beatmap.BeatmapInfo.Difficulty.SliderTickRate, taikoDuration / repeats) / 8;

                if (tickSpacing > 0 && osuDuration < 2 * speedAdjustedBeatLength)
                {
                    for (double j = obj.StartTime; j <= distanceData.EndTime + tickSpacing; j += tickSpacing)
                    {
                        // Todo: This should generate different type of hits (including strongs)
                        // depending on hitobject sound additions (not implemented fully yet)
                        yield return new CentreHit
                        {
                            StartTime = j,
                            Sample = obj.Sample,
                            IsStrong = strong,
                            VelocityMultiplier = legacy_velocity_multiplier
                        };
                    }
                }
                else
                {
                    yield return new DrumRoll
                    {
                        StartTime = obj.StartTime,
                        Sample = obj.Sample,
                        IsStrong = strong,
                        Distance = distance,
                        TickRate = beatmap.BeatmapInfo.Difficulty.SliderTickRate == 3 ? 3 : 4,
                        VelocityMultiplier = legacy_velocity_multiplier
                    };
                }
            }
            else if (endTimeData != null)
            {
                double hitMultiplier = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.Difficulty.OverallDifficulty, 3, 5, 7.5) * swell_hit_multiplier;

                yield return new Swell
                {
                    StartTime = obj.StartTime,
                    Sample = obj.Sample,
                    IsStrong = strong,
                    EndTime = endTimeData.EndTime,
                    RequiredHits = (int)Math.Max(1, endTimeData.Duration / 1000 * hitMultiplier),
                    VelocityMultiplier = legacy_velocity_multiplier
                };
            }
            else
            {
                bool isCentre = (sample & ~(SampleType.Finish | SampleType.Normal)) == 0;

                if (isCentre)
                {
                    yield return new CentreHit
                    {
                        StartTime = obj.StartTime,
                        Sample = obj.Sample,
                        IsStrong = strong,
                        VelocityMultiplier = legacy_velocity_multiplier
                    };
                }
                else
                {
                    yield return new RimHit
                    {
                        StartTime = obj.StartTime,
                        Sample = obj.Sample,
                        IsStrong = strong,
                        VelocityMultiplier = legacy_velocity_multiplier
                    };
                }
            }
        }
    }
}
