// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Converts a Beatmap for another mode.
    /// </summary>
    /// <typeparam name="T">The type of HitObject stored in the Beatmap.</typeparam>
    public abstract class BeatmapConverter<T> : IBeatmapConverter
        where T : HitObject
    {
        private event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;
        event Action<HitObject, IEnumerable<HitObject>> IBeatmapConverter.ObjectConverted
        {
            add => ObjectConverted += value;
            remove => ObjectConverted -= value;
        }

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

        void IBeatmapConverter.Convert(Beatmap original) => Convert(original);

        /// <summary>
        /// Performs the conversion of a Beatmap using this Beatmap Converter.
        /// </summary>
        /// <param name="original">The un-converted Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        protected virtual Beatmap<T> ConvertBeatmap(Beatmap original)
        {
            var beatmap = CreateBeatmap();

            // todo: this *must* share logic (or directly use) Beatmap<T>'s constructor.
            // right now this isn't easily possible due to generic entanglement.
            beatmap.BeatmapInfo = original.BeatmapInfo;
            beatmap.ControlPointInfo = original.ControlPointInfo;
            beatmap.HitObjects = original.HitObjects.SelectMany(h => convert(h, original)).ToList();
            beatmap.Breaks = original.Breaks;

            return beatmap;
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

            var converted = ConvertHitObject(original, beatmap).ToList();
            ObjectConverted?.Invoke(original, converted);

            // Convert the hit object
            foreach (var obj in converted)
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
        /// Creates the <see cref="Beatmap{T}"/> that will be returned by this <see cref="BeatmapProcessor{T}"/>.
        /// </summary>
        protected virtual Beatmap<T> CreateBeatmap() => new Beatmap<T>();

        /// <summary>
        /// Performs the conversion of a hit object.
        /// This method is generally executed sequentially for all objects in a beatmap.
        /// </summary>
        /// <param name="original">The hit object to convert.</param>
        /// <param name="beatmap">The un-converted Beatmap.</param>
        /// <returns>The converted hit object.</returns>
        protected abstract IEnumerable<T> ConvertHitObject(HitObject original, Beatmap beatmap);
    }
}
