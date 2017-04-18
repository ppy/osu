// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Modes.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Converts a Beatmap for another mode.
    /// </summary>
    /// <typeparam name="T">The type of HitObject stored in the Beatmap.</typeparam>
    public interface IBeatmapConverter<T> where T : HitObject
    {
        /// <summary>
        /// The types of HitObjects that can be converted to be used for this Beatmap.
        /// </summary>
        IEnumerable<Type> ValidConversionTypes { get; }

        /// <summary>
        /// Converts a Beatmap to another mode.
        /// </summary>
        /// <param name="original">The original Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        Beatmap<T> Convert(Beatmap original);
    }

    public static class BeatmapConverterExtensions
    {
        /// <summary>
        /// Checks if a Beatmap can be converted using this Beatmap Converter.
        /// </summary>
        /// <param name="converter">The Beatmap Converter.</param>
        /// <param name="beatmap">The Beatmap to check.</param>
        /// <returns>Whether the Beatmap can be converted using <paramref name="converter"/>.</returns>
        public static bool CanConvert<TObject>(this IBeatmapConverter<TObject> converter, Beatmap beatmap) where TObject : HitObject
            => converter.ValidConversionTypes.All(t => beatmap.HitObjects.Any(h => t.IsAssignableFrom(h.GetType())));
    }
}
