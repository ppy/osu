// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
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
        private const float legacy_velocity_scale = 1.4f;
        private const float bash_convert_factor = 1.65f;
        private const float base_scoring_distance = 100;

        /// <summary>
        /// Drum roll distance that results in a duration of 1 speed-adjusted beat length.
        /// </summary>
        private const float base_distance = 100;

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

                double speedAdjustedBeatLength = beatmap.TimingInfo.BeatLengthAt(obj.StartTime) * beatmap.TimingInfo.SpeedMultiplierAt(obj.StartTime);
                double distance = distanceData.Distance * repeats * legacy_velocity_scale;

                double taikoVelocity = base_distance * beatmap.BeatmapInfo.Difficulty.SliderMultiplier / speedAdjustedBeatLength * legacy_velocity_scale;
                double taikoDuration = distance / taikoVelocity;

                double osuVelocity = base_scoring_distance * beatmap.BeatmapInfo.Difficulty.SliderMultiplier / speedAdjustedBeatLength * legacy_velocity_scale;
                double osuDuration = distance / osuVelocity;

                double skipPeriod = Math.Min(speedAdjustedBeatLength / beatmap.BeatmapInfo.Difficulty.SliderTickRate, taikoDuration / repeats);

                if (skipPeriod > 0 && osuDuration < 2 * speedAdjustedBeatLength)
                {
                    for (double j = obj.StartTime; j <= distanceData.EndTime + skipPeriod / 8; j += skipPeriod)
                    {
                        // Todo: This should generate different type of hits (including strongs)
                        // depending on hitobject sound additions (not implemented fully yet)
                        yield return new CentreHit
                        {
                            StartTime = obj.StartTime,
                            Sample = obj.Sample,
                            IsStrong = strong,
                            VelocityMultiplier = legacy_velocity_scale
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
                        Distance = distanceData.Distance * (repeatsData?.RepeatCount ?? 1) * legacy_velocity_scale,
                        TickRate = beatmap.BeatmapInfo.Difficulty.SliderTickRate == 3 ? 3 : 4,
                        VelocityMultiplier = legacy_velocity_scale
                    };
                }
            }
            else if (endTimeData != null)
            {
                double hitMultiplier = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.Difficulty.OverallDifficulty, 3, 5, 7.5) * bash_convert_factor;

                yield return new Swell
                {
                    StartTime = obj.StartTime,
                    Sample = obj.Sample,
                    IsStrong = strong,
                    EndTime = endTimeData.EndTime,
                    RequiredHits = (int)Math.Max(1, endTimeData.Duration / 1000 * hitMultiplier),
                    VelocityMultiplier = legacy_velocity_scale
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
                        VelocityMultiplier = legacy_velocity_scale
                    };
                }
                else
                {
                    yield return new RimHit
                    {
                        StartTime = obj.StartTime,
                        Sample = obj.Sample,
                        IsStrong = strong,
                        VelocityMultiplier = legacy_velocity_scale
                    };
                }
            }
        }
    }
}
