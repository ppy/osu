// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Legacy;
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

        /// <summary>
        /// The total number of columns.
        /// </summary>
        public int TotalColumns => TargetColumns * (Dual ? 2 : 1);

        /// <summary>
        /// The number of columns per-stage.
        /// </summary>
        public int TargetColumns;

        /// <summary>
        /// Whether to double the number of stages.
        /// </summary>
        public bool Dual;

        /// <summary>
        /// Whether the beatmap instantiated with is for the mania ruleset.
        /// </summary>
        public readonly bool IsForCurrentRuleset;

        // Internal for testing purposes
        internal readonly LegacyRandom Random;

        private Pattern lastPattern = new Pattern();

        public ManiaBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : this(beatmap, LegacyBeatmapConversionDifficultyInfo.FromBeatmap(beatmap), ruleset)
        {
        }

        private ManiaBeatmapConverter(IBeatmap? beatmap, LegacyBeatmapConversionDifficultyInfo difficulty, Ruleset ruleset)
            : base(beatmap!, ruleset)
        {
            IsForCurrentRuleset = difficulty.SourceRuleset.Equals(ruleset.RulesetInfo);
            Random = new LegacyRandom((int)MathF.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)MathF.Round(difficulty.ApproachRate));
            TargetColumns = getColumnCount(difficulty);

            if (IsForCurrentRuleset && TargetColumns > ManiaRuleset.MAX_STAGE_KEYS)
            {
                TargetColumns /= 2;
                Dual = true;
            }

            static int getColumnCount(LegacyBeatmapConversionDifficultyInfo difficulty)
            {
                double roundedCircleSize = Math.Round(difficulty.CircleSize);

                if (difficulty.SourceRuleset.ShortName == ManiaRuleset.SHORT_NAME)
                    return (int)Math.Max(1, roundedCircleSize);

                double roundedOverallDifficulty = Math.Round(difficulty.OverallDifficulty);

                if (difficulty.TotalObjectCount > 0 && difficulty.EndTimeObjectCount >= 0)
                {
                    int countSliderOrSpinner = difficulty.EndTimeObjectCount;

                    // In osu!stable, this division appears as if it happens on floats, but due to release-mode
                    // optimisations, it actually ends up happening on doubles.
                    double percentSpecialObjects = (double)countSliderOrSpinner / difficulty.TotalObjectCount;

                    if (percentSpecialObjects < 0.2)
                        return 7;
                    if (percentSpecialObjects < 0.3 || roundedCircleSize >= 5)
                        return roundedOverallDifficulty > 5 ? 7 : 6;
                    if (percentSpecialObjects > 0.6)
                        return roundedOverallDifficulty > 4 ? 5 : 4;
                }

                return Math.Max(4, Math.Min((int)roundedOverallDifficulty + 1, 7));
            }
        }

        public static int GetColumnCount(LegacyBeatmapConversionDifficultyInfo difficulty, IReadOnlyList<Mod>? mods = null)
        {
            var converter = new ManiaBeatmapConverter(null, difficulty, new ManiaRuleset());

            if (mods != null)
            {
                foreach (var m in mods.OfType<IApplicableToBeatmapConverter>())
                    m.ApplyToBeatmapConverter(converter);
            }

            return converter.TotalColumns;
        }

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasXPosition);

        protected override Beatmap<ManiaHitObject> CreateBeatmap()
        {
            ManiaBeatmap beatmap = new ManiaBeatmap(new StageDefinition(TargetColumns));

            if (Dual)
                beatmap.Stages.Add(new StageDefinition(TargetColumns));

            return beatmap;
        }

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            LegacyHitObjectType legacyType;

            switch (original)
            {
                case ManiaHitObject maniaObj:
                {
                    yield return maniaObj;

                    yield break;
                }

                case IHasLegacyHitObjectType legacy:
                    legacyType = legacy.LegacyType & LegacyHitObjectType.ObjectTypes;
                    break;

                case IHasPath:
                    legacyType = LegacyHitObjectType.Slider;
                    break;

                case IHasDuration:
                    legacyType = LegacyHitObjectType.Hold;
                    break;

                default:
                    legacyType = LegacyHitObjectType.Circle;
                    break;
            }

            double startTime = original.StartTime;
            double endTime = (original as IHasDuration)?.EndTime ?? startTime;
            Vector2 position = (original as IHasPosition)?.Position ?? Vector2.Zero;

            PatternGenerator conversion;

            switch (legacyType)
            {
                case LegacyHitObjectType.Circle:
                    if (IsForCurrentRuleset)
                    {
                        conversion = new PassThroughPatternGenerator(Random, original, beatmap, TotalColumns, lastPattern);
                        recordNote(startTime, position);
                    }
                    else
                    {
                        // Note: The density is used during the pattern generator constructor, and intentionally computed first.
                        computeDensity(startTime);
                        conversion = new HitCirclePatternGenerator(Random, original, beatmap, TotalColumns, lastPattern, lastTime, lastPosition, density, lastStair);
                        recordNote(startTime, position);
                    }

                    break;

                case LegacyHitObjectType.Slider:
                    if (IsForCurrentRuleset)
                    {
                        conversion = new PassThroughPatternGenerator(Random, original, beatmap, TotalColumns, lastPattern);
                        recordNote(original.StartTime, position);
                    }
                    else
                    {
                        var generator = new SliderPatternGenerator(Random, original, beatmap, TotalColumns, lastPattern);
                        conversion = generator;

                        for (int i = 0; i <= generator.SpanCount; i++)
                        {
                            double time = original.StartTime + generator.SegmentDuration * i;

                            recordNote(time, position);
                            computeDensity(time);
                        }
                    }

                    break;

                case LegacyHitObjectType.Spinner:
                    // Note: Some older mania-specific beatmaps can have spinners that are converted rather than passed through.
                    //       Newer beatmaps will usually use the "hold" hitobject type below.
                    conversion = new SpinnerPatternGenerator(Random, original, beatmap, TotalColumns, lastPattern);
                    recordNote(endTime, new Vector2(256, 192));
                    computeDensity(endTime);
                    break;

                case LegacyHitObjectType.Hold:
                    conversion = new PassThroughPatternGenerator(Random, original, beatmap, TotalColumns, lastPattern);
                    recordNote(endTime, position);
                    computeDensity(endTime);
                    break;

                default:
                    throw new ArgumentException($"Invalid legacy object type: {legacyType}", nameof(original));
            }

            foreach (var newPattern in conversion.Generate())
            {
                if (conversion is HitCirclePatternGenerator circleGenerator)
                    lastStair = circleGenerator.StairType;

                if (conversion is HitCirclePatternGenerator || conversion is SliderPatternGenerator)
                    lastPattern = newPattern;

                foreach (var obj in newPattern.HitObjects)
                    yield return obj;
            }
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
    }
}
