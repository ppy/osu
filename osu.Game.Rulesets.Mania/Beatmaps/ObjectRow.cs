// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Objects;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class ObjectRow
    {
        private readonly List<ManiaHitObject> hitObjects = new List<ManiaHitObject>();
        public IEnumerable<ManiaHitObject> HitObjects => hitObjects;

        /// <summary>
        /// Whether a column of this row has been taken.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>Whether the column already contains a hit object.</returns>
        public bool IsTaken(int column) => hitObjects.Exists(h => h.Column == column);

        /// <summary>
        /// Amount of columns taken up by hit objects in this row.
        /// </summary>
        public int Columns => HitObjects.GroupBy(h => h.Column).Count();

        /// <summary>
        /// Adds a hit object to this row.
        /// </summary>
        /// <param name="hitObject">The hit object to add.</param>
        public void Add(ManiaHitObject hitObject) => hitObjects.Add(hitObject);

        /// <summary>
        /// Clears this row.
        /// </summary>
        public void Clear() => hitObjects.Clear();

        /// <summary>
        /// Removes a hit object from this row.
        /// </summary>
        /// <param name="hitObject">The hit object to remove.</param>
        public bool Remove(ManiaHitObject hitObject) => hitObjects.Remove(hitObject);
    }
}
