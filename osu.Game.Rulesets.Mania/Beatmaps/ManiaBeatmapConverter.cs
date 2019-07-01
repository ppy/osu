// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;
using osuTK;
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
        public bool Dual;
        public readonly bool IsForCurrentRuleset;

        // Internal for testing purposes
        internal FastRandom Random { get; private set; }

        private Pattern lastPattern = new Pattern();

        private ManiaBeatmap beatmap;

        public ManiaBeatmapConverter(IBeatmap beatmap)
            : base(beatmap)
        {
            IsForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(new ManiaRuleset().RulesetInfo);

            var roundedCircleSize = Math.Round(beatmap.BeatmapInfo.BaseDifficulty.CircleSize);
            var roundedOverallDifficulty = Math.Round(beatmap.BeatmapInfo.BaseDifficulty.OverallDifficulty);

            if (IsForCurrentRuleset)
            {
                TargetColumns = (int)Math.Max(1, roundedCircleSize);

                if (TargetColumns >= 10)
                {
                    TargetColumns = TargetColumns / 2;
                    Dual = true;
                }
            }
            else
            {
                float percentSliderOrSpinner = (float)beatmap.HitObjects.Count(h => h is IHasEndTime) / beatmap.HitObjects.Count;
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

        protected override Beatmap<ManiaHitObject> ConvertBeatmap(IBeatmap original)
        {
            BeatmapDifficulty difficulty = original.BeatmapInfo.BaseDifficulty;

            int seed = (int)Math.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)Math.Round(difficulty.ApproachRate);
            Random = new FastRandom(seed);

            return base.ConvertBeatmap(original);
        }

        protected override Beatmap<ManiaHitObject> CreateBeatmap()
        {
            beatmap = new ManiaBeatmap(new StageDefinition { Columns = TargetColumns });

            if (Dual)
                beatmap.Stages.Add(new StageDefinition { Columns = TargetColumns });

            return beatmap;
        }

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            if (original is ManiaHitObject maniaOriginal)
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
        private PatternType lastStair = PatternType.Stair;

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
        private IEnumerable<ManiaHitObject> generateSpecific(HitObject original, IBeatmap originalBeatmap)
        {
            var generator = new SpecificBeatmapPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);

            foreach (var newPattern in generator.Generate())
            {
                lastPattern = newPattern;

                foreach (var obj in newPattern.HitObjects)
                    yield return obj;
            }
        }

        /// <summary>
        /// Method that generates hit objects for non-osu!mania beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <param name="originalBeatmap">The original beatmap. This is used to look-up any values dependent on a fully-loaded beatmap.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateConverted(HitObject original, IBeatmap originalBeatmap)
        {
            var endTimeData = original as IHasEndTime;
            var distanceData = original as IHasDistance;
            var positionData = original as IHasPosition;

            Patterns.PatternGenerator conversion = null;

            if (distanceData != null)
            {
                var generator = new DistanceObjectPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);
                conversion = generator;

                for (double time = original.StartTime; !Precision.DefinitelyBigger(time, generator.EndTime); time += generator.SegmentDuration)
                {
                    recordNote(time, positionData?.Position ?? Vector2.Zero);
                    computeDensity(time);
                }
            }
            else if (endTimeData != null)
            {
                conversion = new EndTimeObjectPatternGenerator(Random, original, beatmap, originalBeatmap);

                recordNote(endTimeData.EndTime, new Vector2(256, 192));
                computeDensity(endTimeData.EndTime);
            }
            else if (positionData != null)
            {
                computeDensity(original.StartTime);

                conversion = new HitObjectPatternGenerator(Random, original, beatmap, lastPattern, lastTime, lastPosition, density, lastStair, originalBeatmap);

                recordNote(original.StartTime, positionData.Position);
            }

            if (conversion == null)
                yield break;

            foreach (var newPattern in conversion.Generate())
            {
                lastPattern = conversion is EndTimeObjectPatternGenerator ? lastPattern : newPattern;
                lastStair = (conversion as HitObjectPatternGenerator)?.StairType ?? lastStair;

                foreach (var obj in newPattern.HitObjects)
                    yield return obj;
            }
        }

        /// <summary>
        /// A pattern generator for osu!mania-specific beatmaps.
        /// </summary>
        private class SpecificBeatmapPatternGenerator : Patterns.Legacy.PatternGenerator
        {
            public SpecificBeatmapPatternGenerator(FastRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, IBeatmap originalBeatmap)
                : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
            {
            }

            public override IEnumerable<Pattern> Generate()
            {
                yield return generate();
            }

            private Pattern generate()
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
            private List<HitSampleInfo> sampleInfoListAt(double time)
            {
                var curveData = HitObject as IHasCurve;

                if (curveData == null)
                    return HitObject.Samples;

                double segmentTime = (curveData.EndTime - HitObject.StartTime) / curveData.SpanCount();

                int index = (int)(segmentTime == 0 ? 0 : (time - HitObject.StartTime) / segmentTime);
                return curveData.NodeSamples[index];
            }
        }
    }
}
