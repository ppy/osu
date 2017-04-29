// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Beatmaps
{
    /// <summary>
    /// Converts a Beatmap for another mode.
    /// </summary>
    /// <typeparam name="T">The type of HitObject stored in the Beatmap.</typeparam>
    public abstract class BeatmapConverter<T> where T : HitObject
    {
        /// <summary>
        /// Checks if a Beatmap can be converted using this Beatmap Converter.
        /// </summary>
        /// <param name="beatmap">The Beatmap to check.</param>
        /// <returns>Whether the Beatmap can be converted using this Beatmap Converter.</returns>
        public bool CanConvert(Beatmap beatmap) => ValidConversionTypes.All(t => beatmap.HitObjects.Any(t.IsInstanceOfType));

        /// <summary>
        /// Converts a Beatmap using this Beatmap Converter.
        /// </summary>
        /// <param name="original">The un-converted Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        public Beatmap<T> Convert(Beatmap original)
        {
            // We always operate on a clone of the original beatmap, to not modify it game-wide
            return ConvertBeatmap(new Beatmap(original));
        }

        /// <summary>
        /// Performs the conversion of a Beatmap using this Beatmap Converter.
        /// </summary>
        /// <param name="original">The un-converted Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        protected virtual Beatmap<T> ConvertBeatmap(Beatmap original)
        {
            return new Beatmap<T>
            {
                BeatmapInfo = original.BeatmapInfo,
                TimingInfo = original.TimingInfo,
                HitObjects = original.HitObjects.SelectMany(h => convert(h, original)).ToList()
            };
        }

        /// <summary>
        /// Converts a hit object.
        /// </summary>
        /// <param name="original">The hit object to convert.</param>
        /// <param name="beatmap">The un-converted Beatmap.</param>
        /// <returns>The converted hit object.</returns>
        private IEnumerable<T> convert(HitObject original, Beatmap beatmap)
        {
            // Check if the hitobject is already the converted type
            T tObject = original as T;
            if (tObject != null)
            {
                yield return tObject;
                yield break;
            }

            // Convert the hit object
            foreach (var obj in ConvertHitObject(original, beatmap))
            {
                if (obj == null)
                    continue;

                yield return obj;
            }
        }

        /// <summary>
        /// The types of HitObjects that can be converted to be used for this Beatmap.
        /// </summary>
        protected abstract IEnumerable<Type> ValidConversionTypes { get; }

        /// <summary>
        /// Performs the conversion of a hit object.
        /// </summary>
        /// <param name="original">The hit object to convert.</param>
        /// <param name="beatmap">The un-converted Beatmap.</param>
        /// <returns>The converted hit object.</returns>
        protected abstract IEnumerable<T> ConvertHitObject(HitObject original, Beatmap beatmap);
    }
}
