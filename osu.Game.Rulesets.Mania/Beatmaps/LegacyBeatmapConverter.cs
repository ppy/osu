// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Special converter used for converting from osu!stable beatmaps.
    /// </summary>
    internal class LegacyBeatmapConverter
    {
        private Pattern lastPattern = new Pattern();

        private readonly FastRandom random;
        private readonly Beatmap beatmap;

        public LegacyBeatmapConverter(Beatmap beatmap)
        {
            this.beatmap = beatmap;

            int seed = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.DrainRate + beatmap.BeatmapInfo.Difficulty.CircleSize)
                       * 20 + (int)(beatmap.BeatmapInfo.Difficulty.OverallDifficulty * 41.2) + (int)Math.Round(beatmap.BeatmapInfo.Difficulty.ApproachRate);
            random = new FastRandom(seed);
        }

        public IEnumerable<ManiaHitObject> Convert(HitObject original)
        {
            var maniaOriginal = original as ManiaHitObject;
            if (maniaOriginal != null)
            {
                yield return maniaOriginal;
                yield break;
            }

            IEnumerable<ManiaHitObject> objects;
            switch (beatmap.BeatmapInfo.RulesetID)
            {
                default:
                    objects = generateConverted(original);
                    break;
                case 3:
                    objects = generateSpecific(original);
                    break;
            }

            if (objects == null)
                yield break;

            foreach (ManiaHitObject obj in objects)
                yield return obj;
        }

        private IEnumerable<ManiaHitObject> generateSpecific(HitObject original)
        {
            var generator = new SpecificPatternGenerator(random, original, beatmap, lastPattern);

            Pattern newPattern = generator.Generate();
            lastPattern = newPattern;

            return newPattern.HitObjects;
        }

        private IEnumerable<ManiaHitObject> generateConverted(HitObject original)
        {
            var endTimeData = original as IHasEndTime;
            var distanceData = original as IHasDistance;
            var positionData = original as IHasPosition;

            Patterns.PatternGenerator conversion = null;

            if (distanceData != null)
            {
                // Slider
            }
            else if (endTimeData != null)
            {
                conversion = new EndTimeObjectPatternGenerator(random, original, beatmap);
                // Spinner
            }
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
        /// A pattern generator for mania-specific beatmaps.
        /// </summary>
        private class SpecificPatternGenerator : Patterns.Legacy.PatternGenerator
        {
            public SpecificPatternGenerator(FastRandom random, HitObject hitObject, Beatmap beatmap, Pattern previousPattern)
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
