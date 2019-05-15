// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public IBeatmap Beatmap { get; }

        protected BeatmapConverter(IBeatmap beatmap)
        {
            Beatmap = beatmap;
        }

        /// <summary>
        /// Whether <see cref="Beatmap"/> can be converted by this <see cref="BeatmapConverter{T}"/>.
        /// </summary>
        public bool CanConvert => !Beatmap.HitObjects.Any() || ValidConversionTypes.All(t => Beatmap.HitObjects.Any(t.IsInstanceOfType));

        /// <summary>
        /// Converts <see cref="Beatmap"/>.
        /// </summary>
        /// <returns>The converted Beatmap.</returns>
        public IBeatmap Convert()
        {
            // We always operate on a clone of the original beatmap, to not modify it game-wide
            return ConvertBeatmap(Beatmap.Clone());
        }

        /// <summary>
        /// Performs the conversion of a Beatmap using this Beatmap Converter.
        /// </summary>
        /// <param name="original">The un-converted Beatmap.</param>
        /// <returns>The converted Beatmap.</returns>
        protected virtual Beatmap<T> ConvertBeatmap(IBeatmap original)
        {
            var beatmap = CreateBeatmap();

            beatmap.BeatmapInfo = original.BeatmapInfo;
            beatmap.ControlPointInfo = original.ControlPointInfo;
            beatmap.HitObjects = convertHitObjects(original.HitObjects, original);
            beatmap.Breaks = original.Breaks;

            return beatmap;
        }

        private List<T> convertHitObjects(IReadOnlyList<HitObject> hitObjects, IBeatmap beatmap)
        {
            var result = new List<T>(hitObjects.Count);

            foreach (var obj in hitObjects)
            {
                if (obj is T tObj)
                {
                    result.Add(tObj);
                    continue;
                }

                var converted = ConvertHitObject(obj, beatmap);

                if (ObjectConverted != null)
                {
                    converted = converted.ToList();
                    ObjectConverted.Invoke(obj, converted);
                }

                foreach (var c in converted)
                {
                    if (c != null)
                        result.Add(c);
                }
            }

            return result;
        }

        /// <summary>
        /// The types of HitObjects that can be converted to be used for this Beatmap.
        /// </summary>
        protected abstract IEnumerable<Type> ValidConversionTypes { get; }

        /// <summary>
        /// Creates the <see cref="Beatmap{T}"/> that will be returned by this <see cref="BeatmapProcessor"/>.
        /// </summary>
        protected virtual Beatmap<T> CreateBeatmap() => new Beatmap<T>();

        /// <summary>
        /// Performs the conversion of a hit object.
        /// This method is generally executed sequentially for all objects in a beatmap.
        /// </summary>
        /// <param name="original">The hit object to convert.</param>
        /// <param name="beatmap">The un-converted Beatmap.</param>
        /// <returns>The converted hit object.</returns>
        protected abstract IEnumerable<T> ConvertHitObject(HitObject original, IBeatmap beatmap);
    }
}
