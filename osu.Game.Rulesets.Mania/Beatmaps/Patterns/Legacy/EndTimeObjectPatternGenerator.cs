// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    internal class EndTimeObjectPatternGenerator : PatternGenerator
    {
        private readonly double endTime;

        public EndTimeObjectPatternGenerator(FastRandom random, HitObject hitObject, Beatmap beatmap)
            : base(random, hitObject, beatmap, new Pattern())
        {
            var endtimeData = HitObject as IHasEndTime;

            endTime = endtimeData?.EndTime ?? 0;
        }

        public override Pattern Generate()
        {
            var pattern = new Pattern();

            bool generateHold = endTime - HitObject.StartTime >= 100;

            if (AvailableColumns == 8)
            {
                if (HitObject.Samples.Any(s => s.Name == SampleInfo.HIT_FINISH) && endTime - HitObject.StartTime < 1000)
                    addToPattern(pattern, 0, generateHold);
                else
                    addToPattern(pattern, getNextRandomColumn(RandomStart), generateHold);
            }
            else if (AvailableColumns > 0)
                addToPattern(pattern, getNextRandomColumn(0), generateHold);

            return pattern;
        }

        /// <summary>
        /// Picks a random column after a column.
        /// </summary>
        /// <param name="start">The starting column.</param>
        /// <returns>A random column after <paramref name="start"/>.</returns>
        private int getNextRandomColumn(int start)
        {
            int nextColumn = Random.Next(start, AvailableColumns);

            while (PreviousPattern.ColumnHasObject(nextColumn))
                nextColumn = Random.Next(start, AvailableColumns);

            return nextColumn;
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
                    EndSamples = HitObject.Samples,
                    Column = column,
                    Duration = endTime - HitObject.StartTime
                };

                newObject.Samples.Add(new SampleInfo
                {
                    Name = SampleInfo.HIT_NORMAL
                });
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
