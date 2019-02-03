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
        private readonly List<ManiaHitObject> hitObjects = new List<ManiaHitObject>();

        /// <summary>
        /// All the hit objects contained in this pattern.
        /// </summary>
        public IEnumerable<ManiaHitObject> HitObjects => hitObjects;

        /// <summary>
        /// Check whether a column of this patterns contains a hit object.
        /// </summary>
        /// <param name="column">The column index.</param>
        /// <returns>Whether the column with index <paramref name="column"/> contains a hit object.</returns>
        public bool ColumnHasObject(int column) => hitObjects.Exists(h => h.Column == column);

        /// <summary>
        /// Amount of columns taken up by hit objects in this pattern.
        /// </summary>
        public int ColumnWithObjects => HitObjects.GroupBy(h => h.Column).Count();

        /// <summary>
        /// Adds a hit object to this pattern.
        /// </summary>
        /// <param name="hitObject">The hit object to add.</param>
        public void Add(ManiaHitObject hitObject) => hitObjects.Add(hitObject);

        /// <summary>
        /// Copies hit object from another pattern to this one.
        /// </summary>
        /// <param name="other">The other pattern.</param>
        public void Add(Pattern other) => hitObjects.AddRange(other.HitObjects);

        /// <summary>
        /// Clears this pattern, removing all hit objects.
        /// </summary>
        public void Clear() => hitObjects.Clear();

        /// <summary>
        /// Removes a hit object from this pattern.
        /// </summary>
        /// <param name="hitObject">The hit object to remove.</param>
        public bool Remove(ManiaHitObject hitObject) => hitObjects.Remove(hitObject);
    }
}
