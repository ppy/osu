// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns
{
    /// <summary>
    /// Generator to create a pattern <see cref="Pattern"/> from a hit object.
    /// </summary>
    internal abstract class PatternGenerator
    {
        /// <summary>
        /// The number of columns available to create the pattern.
        /// </summary>
        protected readonly int AvailableColumns;

        /// <summary>
        /// The last pattern.
        /// </summary>
        protected readonly Pattern PreviousPattern;

        /// <summary>
        /// The hit object to create the pattern for.
        /// </summary>
        protected readonly HitObject HitObject;
       
        /// <summary>
        /// The beatmap which <see cref="HitObject"/> is a part of.
        /// </summary>
        protected readonly Beatmap Beatmap;

        protected PatternGenerator(HitObject hitObject, Beatmap beatmap, Pattern previousPattern)
        {
            PreviousPattern = previousPattern;
            HitObject = hitObject;
            Beatmap = beatmap;

            AvailableColumns = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.CircleSize);
        }

        /// <summary>
        /// Generates the pattern for <see cref="HitObject"/>, filled with hit objects.
        /// </summary>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        public abstract Pattern Generate();

        /// <summary>
        /// Constructs and adds a note to a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to add to.</param>
        /// <param name="originalObject">The original hit object (used for samples).</param>
        /// <param name="column">The column to add the note to.</param>
        /// <param name="startTime">The start time of the note.</param>
        /// <param name="endTime">The end time of the note (set to <paramref name="startTime"/> for a non-hold note).</param>
        /// <param name="siblings">The number of children alongside this note (these will not be generated, but are used for volume calculations).</param>
        protected void AddToPattern(Pattern pattern, HitObject originalObject, int column, double startTime, double endTime, int siblings = 1)
        {
            ManiaHitObject newObject;

            if (startTime == endTime)
            {
                newObject = new Note
                {
                    StartTime = startTime,
                    Samples = originalObject.Samples,
                    Column = column
                };
            }
            else
            {
                newObject = new HoldNote
                {
                    StartTime = startTime,
                    Samples = originalObject.Samples,
                    Column = column,
                    Duration = endTime - startTime
                };
            }

            // Todo: Consider siblings and write sample volumes (probably at ManiaHitObject level)

            pattern.Add(newObject);
        }
    }
}
