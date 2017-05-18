// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class ObjectList
    {
        private readonly List<ManiaHitObject> hitObjects = new List<ManiaHitObject>();

        /// <summary>
        /// All the hit objects contained in this list.
        /// </summary>
        public IEnumerable<ManiaHitObject> HitObjects => hitObjects;

        /// <summary>
        /// Whether a column of this list has been taken.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>Whether the column already contains a hit object.</returns>
        public bool IsFilled(int column) => hitObjects.Exists(h => h.Column == column);

        /// <summary>
        /// Amount of columns taken up by hit objects in this list.
        /// </summary>
        public int ColumnsFilled => HitObjects.GroupBy(h => h.Column).Count();

        /// <summary>
        /// Adds a hit object to this list.
        /// </summary>
        /// <param name="hitObject">The hit object to add.</param>
        public void Add(ManiaHitObject hitObject) => hitObjects.Add(hitObject);

        /// <summary>
        /// Copies hit object from another list to this one.
        /// </summary>
        /// <param name="other">The other list.</param>
        public void Add(ObjectList other)
        {
            other.HitObjects.ForEach(Add);
        }

        /// <summary>
        /// Clears this list.
        /// </summary>
        public void Clear() => hitObjects.Clear();

        /// <summary>
        /// Removes a hit object from this list.
        /// </summary>
        /// <param name="hitObject">The hit object to remove.</param>
        public bool Remove(ManiaHitObject hitObject) => hitObjects.Remove(hitObject);
    }
}
