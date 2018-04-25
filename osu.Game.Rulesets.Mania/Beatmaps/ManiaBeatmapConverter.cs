// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Linq;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;
using OpenTK;
using osu.Game.Audio;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmapConverter : BeatmapConverter<ManiaHitObject>
    {
        /// <summary>
        /// Maximum number of previous notes to consider for density calculation.
        /// </summary>
        private const int max_notes_for_density = 7;

        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        public int TargetColumns;
        public readonly bool IsForCurrentRuleset;

        private Pattern lastPattern = new Pattern();
        private FastRandom random;

        private ManiaBeatmap beatmap;

        public ManiaBeatmapConverter(bool isForCurrentRuleset, Beatmap original)
        {
            IsForCurrentRuleset = isForCurrentRuleset;

            var roundedCircleSize = Math.Round(original.BeatmapInfo.BaseDifficulty.CircleSize);
            var roundedOverallDifficulty = Math.Round(original.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            if (isForCurrentRuleset)
                TargetColumns = (int)Math.Max(1, roundedCircleSize);
            else
            {
                float percentSliderOrSpinner = (float)original.HitObjects.Count(h => h is IHasEndTime) / original.HitObjects.Count;
                if (percentSliderOrSpinner < 0.2)
                    TargetColumns = 7;
                else if (percentSliderOrSpinner < 0.3 || roundedCircleSize >= 5)
                    TargetColumns = roundedOverallDifficulty > 5 ? 7 : 6;
                else if (percentSliderOrSpinner > 0.6)
                    TargetColumns = roundedOverallDifficulty > 4 ? 5 : 4;
                else
                    TargetColumns = Math.Max(4, Math.Min((int)roundedOverallDifficulty + 1, 7));
            }
        }

        protected override Beatmap<ManiaHitObject> ConvertBeatmap(Beatmap original)
        {
            BeatmapDifficulty difficulty = original.BeatmapInfo.BaseDifficulty;

            int seed = (int)Math.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)Math.Round(difficulty.ApproachRate);
            random = new FastRandom(seed);

            return base.ConvertBeatmap(original);
        }

        protected override Beatmap<ManiaHitObject> CreateBeatmap() => beatmap = new ManiaBeatmap(new StageDefinition { Columns = TargetColumns });

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            var maniaOriginal = original as ManiaHitObject;
            if (maniaOriginal != null)
            {
                yield return maniaOriginal;
                yield break;
            }

            var objects = IsForCurrentRuleset ? generateSpecific(original, beatmap) : generateConverted(original, beatmap);

            if (objects == null)
                yield break;

            foreach (ManiaHitObject obj in objects)
                yield return obj;
        }

        private readonly List<double> prevNoteTimes = new List<double>(max_notes_for_density);
        private double density = int.MaxValue;
        private void computeDensity(double newNoteTime)
        {
            if (prevNoteTimes.Count == max_notes_for_density)
                prevNoteTimes.RemoveAt(0);
            prevNoteTimes.Add(newNoteTime);

            density = (prevNoteTimes[prevNoteTimes.Count - 1] - prevNoteTimes[0]) / prevNoteTimes.Count;
        }

        private double lastTime;
        private Vector2 lastPosition;
        private PatternType lastStair;
        private void recordNote(double time, Vector2 position)
        {
            lastTime = time;
            lastPosition = position;
        }

        /// <summary>
        /// Method that generates hit objects for osu!mania specific beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <param name="originalBeatmap">The original beatmap. This is used to look-up any values dependent on a fully-loaded beatmap.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateSpecific(HitObject original, Beatmap originalBeatmap)
        {
            var generator = new SpecificBeatmapPatternGenerator(random, original, beatmap, lastPattern, originalBeatmap);

            Pattern newPattern = generator.Generate();
            lastPattern = newPattern;

            return newPattern.HitObjects;
        }

        /// <summary>
        /// Method that generates hit objects for non-osu!mania beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <param name="originalBeatmap">The original beatmap. This is used to look-up any values dependent on a fully-loaded beatmap.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateConverted(HitObject original, Beatmap originalBeatmap)
        {
            var endTimeData = original as IHasEndTime;
            var distanceData = original as IHasDistance;
            var positionData = original as IHasPosition;

            Patterns.PatternGenerator conversion = null;

            if (distanceData != null)
                conversion = new DistanceObjectPatternGenerator(random, original, beatmap, lastPattern, originalBeatmap);
            else if (endTimeData != null)
                conversion = new EndTimeObjectPatternGenerator(random, original, beatmap, originalBeatmap);
            else if (positionData != null)
            {
                computeDensity(original.StartTime);

                conversion = new HitObjectPatternGenerator(random, original, beatmap, lastPattern, lastTime, lastPosition, density, lastStair, originalBeatmap);

                recordNote(original.StartTime, positionData.Position);
            }

            if (conversion == null)
                return null;

            Pattern newPattern = conversion.Generate();

            lastPattern = conversion is EndTimeObjectPatternGenerator ? lastPattern : newPattern;
            lastStair = (conversion as HitObjectPatternGenerator)?.StairType ?? lastStair;

            return newPattern.HitObjects;
        }

        /// <summary>
        /// A pattern generator for osu!mania-specific beatmaps.
        /// </summary>
        private class SpecificBeatmapPatternGenerator : Patterns.Legacy.PatternGenerator
        {
            public SpecificBeatmapPatternGenerator(FastRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, Beatmap originalBeatmap)
                : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
            {
            }

            public override Pattern Generate()
            {
                var endTimeData = HitObject as IHasEndTime;
                var positionData = HitObject as IHasXPosition;

                int column = GetColumn(positionData?.X ?? 0);

                var pattern = new Pattern();

                if (endTimeData != null)
                {
                    pattern.Add(new HoldNote
                    {
                        StartTime = HitObject.StartTime,
                        Duration = endTimeData.Duration,
                        Column = column,
                        Head = { Samples = sampleInfoListAt(HitObject.StartTime) },
                        Tail = { Samples = sampleInfoListAt(endTimeData.EndTime) },
                    });
                }
                else if (positionData != null)
                {
                    pattern.Add(new Note
                    {
                        StartTime = HitObject.StartTime,
                        Samples = HitObject.Samples,
                        Column = column
                    });
                }

                return pattern;
            }

            /// <summary>
            /// Retrieves the sample info list at a point in time.
            /// </summary>
            /// <param name="time">The time to retrieve the sample info list from.</param>
            /// <returns></returns>
            private List<SampleInfo> sampleInfoListAt(double time)
            {
                var curveData = HitObject as IHasCurve;

                if (curveData == null)
                    return HitObject.Samples;

                double segmentTime = (curveData.EndTime - HitObject.StartTime) / curveData.SpanCount();

                int index = (int)(segmentTime == 0 ? 0 : (time - HitObject.StartTime) / segmentTime);
                return curveData.RepeatSamples[index];
            }
        }
    }
}
