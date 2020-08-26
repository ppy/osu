// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// A row of statistics to be displayed on the results screen.
    /// </summary>
    public interface IStatisticRow
    {
        /// <summary>
        /// Creates the visual representation of this row.
        /// </summary>
        Drawable CreateDrawableStatisticRow();
    }
}
