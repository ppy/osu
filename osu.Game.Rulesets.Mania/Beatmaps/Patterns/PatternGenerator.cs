// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns
{
    /// <summary>
    /// Generator to create a pattern <see cref="Pattern"/> from a hit object.
    /// </summary>
    internal abstract class PatternGenerator
    {
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
        protected readonly ManiaBeatmap Beatmap;

        protected readonly int TotalColumns;

        protected PatternGenerator(HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern)
        {
            ArgumentNullException.ThrowIfNull(hitObject);
            ArgumentNullException.ThrowIfNull(beatmap);
            ArgumentNullException.ThrowIfNull(previousPattern);

            HitObject = hitObject;
            Beatmap = beatmap;
            PreviousPattern = previousPattern;

            TotalColumns = Beatmap.TotalColumns;
        }

        /// <summary>
        /// Generates the patterns for <see cref="HitObject"/>, each filled with hit objects.
        /// </summary>
        /// <returns>The <see cref="Pattern"/>s containing the hit objects.</returns>
        public abstract IEnumerable<Pattern> Generate();
    }
}
