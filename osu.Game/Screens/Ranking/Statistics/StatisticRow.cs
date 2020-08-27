// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// A row of statistics to be displayed in the results screen.
    /// </summary>
    public class StatisticRow
    {
        /// <summary>
        /// The columns of this <see cref="StatisticRow"/>.
        /// </summary>
        [ItemNotNull]
        public StatisticItem[] Columns;
    }
}
