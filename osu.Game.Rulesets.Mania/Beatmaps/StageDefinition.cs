// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    /// <summary>
    /// Defines properties for each stage in a <see cref="ManiaPlayfield"/>.
    /// </summary>
    public struct StageDefinition
    {
        /// <summary>
        /// The number of <see cref="Column"/>s which this stage contains.
        /// </summary>
        public int Columns;

        /// <summary>
        /// Get the <see cref="ColumnType"/> for a given column index.
        /// </summary>
        /// <param name="column">The 0-based column index.</param>
        /// <returns>The type of the column.</returns>
        public ColumnType TypeOfColumn(int column)
        {
            if (Columns % 2 == 1 && column == Columns / 2)
                return ColumnType.Special;

            var distanceToEdge = Math.Min(column, Columns - column - 1);

            return distanceToEdge % 2 == 0 ? ColumnType.Odd : ColumnType.Even;
        }
    }
}
