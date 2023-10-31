// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    internal class EndTimeObjectPatternGenerator : PatternGenerator
    {
        private readonly int endTime;
        private readonly PatternType convertType;

        public EndTimeObjectPatternGenerator(LegacyRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, IBeatmap originalBeatmap)
            : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
        {
            endTime = (int)((HitObject as IHasDuration)?.EndTime ?? 0);

            convertType = PreviousPattern.ColumnWithObjects == TotalColumns
                ? PatternType.None
                : PatternType.ForceNotStack;
        }

        public override IEnumerable<Pattern> Generate()
        {
            yield return generate();
        }

        private Pattern generate()
        {
            var pattern = new Pattern();

            bool generateHold = endTime - HitObject.StartTime >= 100;

            switch (TotalColumns)
            {
                case 8 when HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH) && endTime - HitObject.StartTime < 1000:
                    addToPattern(pattern, 0, generateHold);
                    break;

                case 8:
                    addToPattern(pattern, getRandomColumn(), generateHold);
                    break;

                default:
                    addToPattern(pattern, getRandomColumn(0), generateHold);
                    break;
            }

            return pattern;
        }

        private int getRandomColumn(int? lowerBound = null)
        {
            if ((convertType & PatternType.ForceNotStack) > 0)
                return FindAvailableColumn(GetRandomColumn(lowerBound), lowerBound, patterns: PreviousPattern);

            return FindAvailableColumn(GetRandomColumn(lowerBound), lowerBound);
        }

        /// <summary>
        /// Constructs and adds a note to a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to add to.</param>
        /// <param name="column">The column to add the note to.</param>
        /// <param name="holdNote">Whether to add a hold note.</param>
        private void addToPattern(Pattern pattern, int column, bool holdNote)
        {
            ManiaHitObject newObject;

            if (holdNote)
            {
                newObject = new HoldNote
                {
                    StartTime = HitObject.StartTime,
                    Duration = endTime - HitObject.StartTime,
                    Column = column,
                    Samples = HitObject.Samples,
                    NodeSamples = (HitObject as IHasRepeats)?.NodeSamples
                };
            }
            else
            {
                newObject = new Note
                {
                    StartTime = HitObject.StartTime,
                    Samples = HitObject.Samples,
                    Column = column
                };
            }

            pattern.Add(newObject);
        }
    }
}
