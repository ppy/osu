// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticRow
    {
        /// <summary>
        /// The columns of this <see cref="StatisticRow"/>.
        /// </summary>
        [ItemCanBeNull]
        public StatisticItem[] Columns;
    }
}
