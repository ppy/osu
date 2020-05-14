// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Taiko.Beatmaps
{
    internal class TaikoBeatmapConverter : BeatmapConverter<TaikoHitObject>
    {
        /// <summary>
        /// osu! is generally slower than taiko, so a factor is added to increase
        /// speed. This must be used everywhere slider length or beat length is used.
        /// </summary>
        public const float LEGACY_VELOCITY_MULTIPLIER = 1.4f;

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

        private readonly bool isForCurrentRuleset;

        public TaikoBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
        }

        public override bool CanConvert() => true;

        protected override Beatmap<TaikoHitObject> ConvertBeatmap(IBeatmap original)
        {
            // Rewrite the beatmap info to add the slider velocity multiplier
            original.BeatmapInfo = original.BeatmapInfo.Clone();
            original.BeatmapInfo.BaseDifficulty = original.BeatmapInfo.BaseDifficulty.Clone();
            original.BeatmapInfo.BaseDifficulty.SliderMultiplier *= LEGACY_VELOCITY_MULTIPLIER;

            Beatmap<TaikoHitObject> converted = base.ConvertBeatmap(original);

            if (original.BeatmapInfo.RulesetID == 3)
            {
                // Post processing step to transform mania hit objects with the same start time into strong hits
                converted.HitObjects = converted.HitObjects.GroupBy(t => t.StartTime).Select(x =>
                {
                    TaikoHitObject first = x.First();
                    if (x.Skip(1).Any() && !(first is Swell))
                        first.IsStrong = true;
                    return first;
                }).ToList();
            }

            return converted;
        }

        protected override IEnumerable<TaikoHitObject> ConvertHitObject(HitObject obj, IBeatmap beatmap)
        {
            // Old osu! used hit sounding to determine various hit type information
            IList<HitSampleInfo> samples = obj.Samples;

            bool strong = samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);

            switch (obj)
            {
                case IHasDistance distanceData:
                {
                    // Number of spans of the object - one for the initial length and for each repeat
                    int spans = (obj as IHasRepeats)?.SpanCount() ?? 1;

                    TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(obj.StartTime);
                    DifficultyControlPoint difficultyPoint = beatmap.ControlPointInfo.DifficultyPointAt(obj.StartTime);

                    double speedAdjustment = difficultyPoint.SpeedMultiplier;
                    double speedAdjustedBeatLength = timingPoint.BeatLength / speedAdjustment;

                    // The true distance, accounting for any repeats. This ends up being the drum roll distance later
                    double distance = distanceData.Distance * spans * LEGACY_VELOCITY_MULTIPLIER;

                    // The velocity of the taiko hit object - calculated as the velocity of a drum roll
                    double taikoVelocity = taiko_base_distance * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier / speedAdjustedBeatLength;
                    // The duration of the taiko hit object
                    double taikoDuration = distance / taikoVelocity;

                    // The velocity of the osu! hit object - calculated as the velocity of a slider
                    double osuVelocity = osu_base_scoring_distance * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier / speedAdjustedBeatLength;
                    // The duration of the osu! hit object
                    double osuDuration = distance / osuVelocity;

                    // osu-stable always uses the speed-adjusted beatlength to determine the velocities, but
                    // only uses it for tick rate if beatmap version < 8
                    if (beatmap.BeatmapInfo.BeatmapVersion >= 8)
                        speedAdjustedBeatLength *= speedAdjustment;

                    // If the drum roll is to be split into hit circles, assume the ticks are 1/8 spaced within the duration of one beat
                    double tickSpacing = Math.Min(speedAdjustedBeatLength / beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate, taikoDuration / spans);

                    if (!isForCurrentRuleset && tickSpacing > 0 && osuDuration < 2 * speedAdjustedBeatLength)
                    {
                        List<IList<HitSampleInfo>> allSamples = obj is IHasCurve curveData ? curveData.NodeSamples : new List<IList<HitSampleInfo>>(new[] { samples });

                        int i = 0;

                        for (double j = obj.StartTime; j <= obj.StartTime + taikoDuration + tickSpacing / 8; j += tickSpacing)
                        {
                            IList<HitSampleInfo> currentSamples = allSamples[i];
                            bool isRim = currentSamples.Any(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE);
                            strong = currentSamples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);

                            yield return new Hit
                            {
                                StartTime = j,
                                Type = isRim ? HitType.Rim : HitType.Centre,
                                Samples = currentSamples,
                                IsStrong = strong
                            };

                            i = (i + 1) % allSamples.Count;
                        }
                    }
                    else
                    {
                        yield return new DrumRoll
                        {
                            StartTime = obj.StartTime,
                            Samples = obj.Samples,
                            IsStrong = strong,
                            Duration = taikoDuration,
                            TickRate = beatmap.BeatmapInfo.BaseDifficulty.SliderTickRate == 3 ? 3 : 4
                        };
                    }

                    break;
                }

                case IHasEndTime endTimeData:
                {
                    double hitMultiplier = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty, 3, 5, 7.5) * swell_hit_multiplier;

                    yield return new Swell
                    {
                        StartTime = obj.StartTime,
                        Samples = obj.Samples,
                        Duration = endTimeData.Duration,
                        RequiredHits = (int)Math.Max(1, endTimeData.Duration / 1000 * hitMultiplier)
                    };

                    break;
                }

                default:
                {
                    bool isRimDefinition(HitSampleInfo s) => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE;

                    bool isRim = samples.Any(isRimDefinition);

                    yield return new Hit
                    {
                        StartTime = obj.StartTime,
                        Type = isRim ? HitType.Rim : HitType.Centre,
                        Samples = samples,
                        IsStrong = strong
                    };

                    break;
                }
            }
        }

        protected override Beatmap<TaikoHitObject> CreateBeatmap() => new TaikoBeatmap();
    }
}
