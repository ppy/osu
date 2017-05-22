// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    public class ManiaBeatmapConverter : BeatmapConverter<ManiaHitObject>
    {
        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasXPosition) };

        private Pattern lastPattern = new Pattern();
        private FastRandom random;
        private Beatmap beatmap;
        private bool isForCurrentRuleset;

        protected override Beatmap<ManiaHitObject> ConvertBeatmap(Beatmap original, bool isForCurrentRuleset)
        {
            this.isForCurrentRuleset = isForCurrentRuleset;

            beatmap = original;

            BeatmapDifficulty difficulty = original.BeatmapInfo.Difficulty;

            int seed = (int)Math.Round(difficulty.DrainRate + difficulty.CircleSize) * 20 + (int)(difficulty.OverallDifficulty * 41.2) + (int)Math.Round(difficulty.ApproachRate);
            random = new FastRandom(seed);

            return base.ConvertBeatmap(original, isForCurrentRuleset);
        }

        protected override IEnumerable<ManiaHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
        {
            var maniaOriginal = original as ManiaHitObject;
            if (maniaOriginal != null)
            {
                yield return maniaOriginal;
                yield break;
            }

            var objects = isForCurrentRuleset ? generateSpecific(original) : generateConverted(original);

            if (objects == null)
                yield break;

            foreach (ManiaHitObject obj in objects)
                yield return obj;
        }

        /// <summary>
        /// Method that generates hit objects for osu!mania specific beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateSpecific(HitObject original)
        {
            var generator = new SpecificBeatmapPatternGenerator(random, original, beatmap, lastPattern);

            Pattern newPattern = generator.Generate();
            lastPattern = newPattern;

            return newPattern.HitObjects;
        }

        /// <summary>
        /// Method that generates hit objects for non-osu!mania beatmaps.
        /// </summary>
        /// <param name="original">The original hit object.</param>
        /// <returns>The hit objects generated.</returns>
        private IEnumerable<ManiaHitObject> generateConverted(HitObject original)
        {
            var endTimeData = original as IHasEndTime;
            var distanceData = original as IHasDistance;
            var positionData = original as IHasPosition;

            // Following lines currently commented out to appease resharper

            Patterns.PatternGenerator conversion = null;

            if (distanceData != null)
                conversion = new DistanceObjectPatternGenerator(random, original, beatmap, lastPattern);
            else if (endTimeData != null)
                conversion = new EndTimeObjectPatternGenerator(random, original, beatmap);
            else if (positionData != null)
            {
                // Circle
            }

            if (conversion == null)
                return null;

            Pattern newPattern = conversion.Generate();
            lastPattern = newPattern;

            return newPattern.HitObjects;
        }

        /// <summary>
        /// A pattern generator for osu!mania-specific beatmaps.
        /// </summary>
        private class SpecificBeatmapPatternGenerator : Patterns.Legacy.PatternGenerator
        {
            public SpecificBeatmapPatternGenerator(FastRandom random, HitObject hitObject, Beatmap beatmap, Pattern previousPattern)
                : base(random, hitObject, beatmap, previousPattern)
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
                        Samples = HitObject.Samples,
                        Duration = endTimeData.Duration,
                        Column = column,
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
        }
    }
}
