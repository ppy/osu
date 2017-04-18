// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Modes.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Beatmaps
{
    /// <summary>
    /// Converts a Beatmap for another mode.
    /// </summary>
    /// <typeparam name="T">The type of HitObject stored in the Beatmap.</typeparam>
    public abstract class BeatmapConverter<T> where T : HitObject
    {
        /// <summary>
        /// The types of HitObjects that can be converted to be used for this Beatmap.
        /// </summary>
        public abstract IEnumerable<Type> ValidConversionTypes { get; }

        /// <summary>
        /// Converts a Beatmap to another mode.
        /// </summary>
        /// <param name="original">The original Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        public abstract Beatmap<T> Convert(Beatmap original);

        /// <summary>
        /// Checks if a Beatmap can be converted using this Beatmap Converter.
        /// </summary>
        /// <param name="beatmap">The Beatmap to check.</param>
        /// <returns>Whether the Beatmap can be converted using this Beatmap Converter.</returns>
        public bool CanConvert(Beatmap beatmap) => ValidConversionTypes.All(t => beatmap.HitObjects.Any(h => t.IsAssignableFrom(h.GetType())));
    }
}
