// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;
using osu.Game.Rulesets.Scoring.Legacy;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmapConverter : BeatmapConverter<ManiaHitObject>
    {
        /// <summary>
        /// Maximum number of previous notes to consider for density calculation.
        /// </summary>
        private const int max_notes_for_density = 7;

        public int TargetColumns;
        public bool Dual;
        public readonly bool IsForCurrentRuleset;

        private readonly int originalTargetColumns;

        // Internal for testing purposes
        internal LegacyRandom Random { get; private set; }

        private Pattern lastPattern = new Pattern();

        private ManiaBeatmap beatmap;

        public ManiaBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
            IsForCurrentRuleset = beatmap.BeatmapInfo.Ruleset.Equals(ruleset.RulesetInfo);
            TargetColumns = GetColumnCount(LegacyBeatmapConversionDifficultyInfo.FromBeatmap(beatmap));

            if (IsForCurrentRuleset && TargetColumns > ManiaRuleset.MAX_STAGE_KEYS)
            {
                TargetColumns /= 2;
                Dual = true;
            }

            originalTargetColumns = TargetColumns;
        }

        public static int GetColumnCount(LegacyBeatmapConversionDifficultyInfo difficulty)
        {
            if (new ManiaRuleset().RulesetInfo.Equals(difficulty.SourceRuleset))
                return GetColumnCountForNonConvert(difficulty);

            double roundedCircleSize = Math.Round(difficulty.CircleSize);
            double roundedOverallDifficulty = Math.Round(difficulty.OverallDifficulty);

            int countSliderOrSpinner = difficulty.TotalObjectCount - difficulty.CircleCount;
            float percentSpecialObjects = (float)countSliderOrSpinner / difficulty.TotalObjectCount;

            if (percentSpecialObjects < 0.2)
                return 7;
            if (percentSpecialObjects < 0.3 || roundedCircleSize >= 5)
                return roundedOverallDifficulty > 5 ? 7 : 6;
            if (percentSpecialObjects > 0.6)
                return roundedOverallDifficulty > 4 ? 5 : 4;

            return Math.Max(4, Math.Min((int)roundedOverallDifficulty + 1, 7));
        }

        public static int GetColumnCountForNonConvert(IBeatmapDifficultyInfo difficulty)
        {
            double roundedCircleSize = Math.Round(difficulty.CircleSize);
            return (int)Math.Max(1, roundedCircleSize);
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasXPosition);

        protected override Beatmap<ManiaHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            IBeatmapDifficultyInfo difficulty = original.Difficulty;

            int seed = (int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate);
            Random = new LegacyRandom(seed);

            return base.ConvertBeatmap(original, cancellationToken);
        }

        protected override Beatmap<ManiaHitObject> CreateBeatmap()
        {
            beatmap = new ManiaBeatmap(new StageDefinition(TargetColumns), originalTargetColumns);

            if (Dual)
                beatmap.Stages.Add(new StageDefinition(TargetColumns));

            return beatmap;
        }

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
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

        private readonly LimitedCapacityQueue<double> prevNoteTimes = new LimitedCapacityQueue<double>(max_notes_for_density);
        private double density = int.MaxValue;

        private void computeDensity(double newNoteTime)
        {
            prevNoteTimes.Enqueue(newNoteTime);

            if (prevNoteTimes.Count >= 2)
                density = (prevNoteTimes[^1] - prevNoteTimes[0]) / prevNoteTimes.Count;
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
            Patterns.PatternGenerator conversion = null;

            switch (original)
            {
                case IHasPath:
                {
                    var generator = new PathObjectPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);
                    conversion = generator;

                    var positionData = original as IHasPosition;

                    for (int i = 0; i <= generator.SpanCount; i++)
                    {
                        double time = original.StartTime + generator.SegmentDuration * i;

                        recordNote(time, positionData?.Position ?? Vector2.Zero);
                        computeDensity(time);
                    }

                    break;
                }

                case IHasDuration endTimeData:
                {
                    conversion = new EndTimeObjectPatternGenerator(Random, original, beatmap, lastPattern, originalBeatmap);

                    recordNote(endTimeData.EndTime, new Vector2(256, 192));
                    computeDensity(endTimeData.EndTime);
                    break;
                }

                case IHasPosition positionData:
                {
                    computeDensity(original.StartTime);

                    conversion = new HitObjectPatternGenerator(Random, original, beatmap, lastPattern, lastTime, lastPosition, density, lastStair, originalBeatmap);

                    recordNote(original.StartTime, positionData.Position);
                    break;
                }
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
            public SpecificBeatmapPatternGenerator(LegacyRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, IBeatmap originalBeatmap)
                : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
            {
            }

            public override IEnumerable<Pattern> Generate()
            {
                yield return generate();
            }

            private Pattern generate()
            {
                var positionData = HitObject as IHasXPosition;

                int column = GetColumn(positionData?.X ?? 0);

                var pattern = new Pattern();

                if (HitObject is IHasDuration endTimeData)
                {
                    pattern.Add(new HoldNote
                    {
                        StartTime = HitObject.StartTime,
                        Duration = endTimeData.Duration,
                        Column = column,
                        Samples = HitObject.Samples,
                        NodeSamples = (HitObject as IHasRepeats)?.NodeSamples ?? defaultNodeSamples
                    });
                }
                else if (HitObject is IHasXPosition)
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

            /// <remarks>
            /// osu!mania-specific beatmaps in stable only play samples at the start of the hold note.
            /// </remarks>
            private List<IList<HitSampleInfo>> defaultNodeSamples
                => new List<IList<HitSampleInfo>>
                {
                    HitObject.Samples,
                    new List<HitSampleInfo>()
                };
        }
    }
}
