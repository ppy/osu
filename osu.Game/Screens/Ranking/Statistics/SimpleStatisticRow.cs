// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Contains textual statistic data to display in a <see cref="DrawableSimpleStatisticRow"/>.
    /// </summary>
    public class SimpleStatisticRow
    {
        /// <summary>
        /// The number of columns to layout the <see cref="Items"/> in.
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// The <see cref="StatisticItem"/>s that this row should contain.
        /// </summary>
        [ItemNotNull]
        public SimpleStatisticItem[] Items { get; set; }

        public Drawable CreateDrawableStatisticRow() => new Container
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Padding = new MarginPadding(20),
            Child = new DrawableSimpleStatisticRow(Columns, Items)
        };
    }
}
