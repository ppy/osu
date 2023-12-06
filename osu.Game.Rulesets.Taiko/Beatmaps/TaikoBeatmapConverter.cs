// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Taiko.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Objects.Legacy;

namespace osu.Game.Rulesets.Taiko.Beatmaps
{
    internal class TaikoBeatmapConverter : BeatmapConverter<TaikoHitObject>
    {
        /// <summary>
        /// Because swells are easier in taiko than spinners are in osu!,
        /// legacy taiko multiplies a factor when converting the number of required hits.
        /// </summary>
        private const float swell_hit_multiplier = 1.65f;

        /// <summary>
        /// Base osu! slider scoring distance.
        /// </summary>
        private const float osu_base_scoring_distance = 100;

        private readonly bool isForCurrentRuleset;

        public TaikoBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            isForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
        }

        public override bool CanConvert() => true;

        protected override Beatmap<TaikoHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            Beatmap<TaikoHitObject> converted = base.ConvertBeatmap(original, cancellationToken);

            if (original.BeatmapInfo.Ruleset.OnlineID == 0)
            {
                // Post processing step to transform standard slider velocity changes into scroll speed changes
                double lastScrollSpeed = 1;

                foreach (HitObject hitObject in original.HitObjects)
                {
                    if (hitObject is not IHasSliderVelocity hasSliderVelocity) continue;

                    double nextScrollSpeed = hasSliderVelocity.SliderVelocityMultiplier;
                    EffectControlPoint currentEffectPoint = converted.ControlPointInfo.EffectPointAt(hitObject.StartTime);

                    if (!Precision.AlmostEquals(lastScrollSpeed, nextScrollSpeed, acceptableDifference: currentEffectPoint.ScrollSpeedBindable.Precision))
                    {
                        converted.ControlPointInfo.Add(hitObject.StartTime, new EffectControlPoint
                        {
                            KiaiMode = currentEffectPoint.KiaiMode,
                            ScrollSpeed = lastScrollSpeed = nextScrollSpeed,
                        });
                    }
                }
            }

            if (original.BeatmapInfo.Ruleset.OnlineID == 3)
            {
                // Post processing step to transform mania hit objects with the same start time into strong hits
                converted.HitObjects = converted.HitObjects.GroupBy(t => t.StartTime).Select(x =>
                {
                    TaikoHitObject first = x.First();
                    if (x.Skip(1).Any() && first is TaikoStrongableHitObject strong)
                        strong.IsStrong = true;
                    return first;
                }).ToList();
            }

            // TODO: stable makes the last tick of a drumroll non-required when the next object is too close.
            // This probably needs to be reimplemented:
            //
            // List<HitObject> hitobjects = hitObjectManager.hitObjects;
            // int ind = hitobjects.IndexOf(this);
            // if (i < hitobjects.Count - 1 && hitobjects[i + 1].HittableStartTime - (EndTime + (int)TickSpacing) <= (int)TickSpacing)
            //     lastTickHittable = false;

            return converted;
        }

        protected override IEnumerable<TaikoHitObject> ConvertHitObject(HitObject obj, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            // Old osu! used hit sounding to determine various hit type information
            IList<HitSampleInfo> samples = obj.Samples;

            switch (obj)
            {
                case IHasPath pathData:
                {
                    if (shouldConvertSliderToHits(obj, beatmap, pathData, out int taikoDuration, out double tickSpacing))
                    {
                        IList<IList<HitSampleInfo>> allSamples = obj is IHasPathWithRepeats curveData ? curveData.NodeSamples : new List<IList<HitSampleInfo>>(new[] { samples });

                        int i = 0;

                        for (double j = obj.StartTime; j <= obj.StartTime + taikoDuration + tickSpacing / 8; j += tickSpacing)
                        {
                            IList<HitSampleInfo> currentSamples = allSamples[i];

                            yield return new Hit
                            {
                                StartTime = j,
                                Samples = currentSamples,
                            };

                            i = (i + 1) % allSamples.Count;

                            if (Precision.AlmostEquals(0, tickSpacing))
                                break;
                        }
                    }
                    else
                    {
                        yield return new DrumRoll
                        {
                            StartTime = obj.StartTime,
                            Samples = obj.Samples,
                            Duration = taikoDuration,
                        };
                    }

                    break;
                }

                case IHasDuration endTimeData:
                {
                    double hitMultiplier = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.OverallDifficulty, 3, 5, 7.5) * swell_hit_multiplier;

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
                    yield return new Hit
                    {
                        StartTime = obj.StartTime,
                        Samples = samples,
                    };

                    break;
                }
            }
        }

        private bool shouldConvertSliderToHits(HitObject obj, IBeatmap beatmap, IHasPath pathData, out int taikoDuration, out double tickSpacing)
        {
            // DO NOT CHANGE OR REFACTOR ANYTHING IN HERE WITHOUT TESTING AGAINST _ALL_ BEATMAPS.
            // Some of these calculations look redundant, but they are not - extremely small floating point errors are introduced to maintain 1:1 compatibility with stable.
            // Rounding cannot be used as an alternative since the error deltas have been observed to be between 1e-2 and 1e-6.

            // The true distance, accounting for any repeats. This ends up being the drum roll distance later
            int spans = (obj as IHasRepeats)?.SpanCount() ?? 1;
            double distance = pathData.Path.ExpectedDistance.Value ?? 0;

            // Do not combine the following two lines!
            distance *= LegacyBeatmapEncoder.LEGACY_TAIKO_VELOCITY_MULTIPLIER;
            distance *= spans;

            TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(obj.StartTime);

            double beatLength;

            if (obj is IHasSliderVelocity hasSliderVelocity)
                beatLength = LegacyRulesetExtensions.GetPrecisionAdjustedBeatLength(hasSliderVelocity, timingPoint, TaikoRuleset.SHORT_NAME);
            else
                beatLength = timingPoint.BeatLength;

            double sliderScoringPointDistance = osu_base_scoring_distance * (beatmap.Difficulty.SliderMultiplier * LegacyBeatmapEncoder.LEGACY_TAIKO_VELOCITY_MULTIPLIER) / beatmap.Difficulty.SliderTickRate;

            // The velocity and duration of the taiko hit object - calculated as the velocity of a drum roll.
            double taikoVelocity = sliderScoringPointDistance * beatmap.Difficulty.SliderTickRate;
            taikoDuration = (int)(distance / taikoVelocity * beatLength);

            if (isForCurrentRuleset)
            {
                tickSpacing = 0;
                return false;
            }

            double osuVelocity = taikoVelocity * (1000f / beatLength);

            // osu-stable always uses the speed-adjusted beatlength to determine the osu! velocity, but only uses it for conversion if beatmap version < 8
            if (beatmap.BeatmapInfo.BeatmapVersion >= 8)
                beatLength = timingPoint.BeatLength;

            // If the drum roll is to be split into hit circles, assume the ticks are 1/8 spaced within the duration of one beat
            tickSpacing = Math.Min(beatLength / beatmap.Difficulty.SliderTickRate, (double)taikoDuration / spans);

            return tickSpacing > 0
                   && distance / osuVelocity * 1000 < 2 * beatLength;
        }

        protected override Beatmap<TaikoHitObject> CreateBeatmap() => new TaikoBeatmap();
    }
}
