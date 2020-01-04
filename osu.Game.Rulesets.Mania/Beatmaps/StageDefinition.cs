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
        /// Whether the column index is a special column for this stage.
        /// </summary>
        /// <param name="column">The 0-based column index.</param>
        /// <returns>Whether the column is a special column.</returns>
        public bool IsSpecialColumn(int column)
            // TODO: Inline this method.
            => Columns % 2 == 1 && column == Columns / 2;

        public ColumnType TypeOfColumn(int column)
        {
            if (IsSpecialColumn(column))
                return ColumnType.Special;

            var distanceToEdge = Math.Min(column, Columns - column - 1);

            return distanceToEdge % 2 == 0 ? ColumnType.Odd : ColumnType.Even;
        }
    }
}
