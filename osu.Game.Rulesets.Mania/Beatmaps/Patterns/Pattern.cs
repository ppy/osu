// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns
{
    /// <summary>
    /// Creates a pattern containing hit objects.
    /// </summary>
    internal class Pattern
    {
        private List<ManiaHitObject> hitObjects;
        private HashSet<int> containedColumns;

        /// <summary>
        /// All the hit objects contained in this pattern.
        /// </summary>
        public IEnumerable<ManiaHitObject> HitObjects => hitObjects ?? Enumerable.Empty<ManiaHitObject>();

        /// <summary>
        /// Check whether a column of this patterns contains a hit object.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>Whether the column with index <paramref name="column"/> contains a hit object.</returns>
        public bool ColumnHasObject(int column) => containedColumns?.Contains(column) == true;

        /// <summary>
        /// Amount of columns taken up by hit objects in this pattern.
        /// </summary>
        public int ColumnWithObjects => containedColumns?.Count ?? 0;

        /// <summary>
        /// Adds a hit object to this pattern.
        /// </summary>
        /// <param name="hitObject">The hit object to add.</param>
        public void Add(ManiaHitObject hitObject)
        {
            prepareStorage();

            hitObjects.Add(hitObject);
            containedColumns.Add(hitObject.Column);
        }

        /// <summary>
        /// Copies hit object from another pattern to this one.
        /// </summary>
        /// <param name="other">The other pattern.</param>
        public void Add(Pattern other)
        {
            prepareStorage();

            if (other.hitObjects != null)
            {
                hitObjects.AddRange(other.hitObjects);

                foreach (var h in other.hitObjects)
                    containedColumns.Add(h.Column);
            }
        }

        /// <summary>
        /// Clears this pattern, removing all hit objects.
        /// </summary>
        public void Clear()
        {
            hitObjects?.Clear();
            containedColumns?.Clear();
        }

        private void prepareStorage()
        {
            hitObjects ??= new List<ManiaHitObject>();
            containedColumns ??= new HashSet<int>();
        }
    }
}
