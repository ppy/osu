// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
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
    }
}
