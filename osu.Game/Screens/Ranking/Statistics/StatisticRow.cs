// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// A row of graphically detailed <see cref="StatisticItem"/>s to be displayed in the results screen.
    /// </summary>
    public class StatisticRow : IStatisticRow
    {
        /// <summary>
        /// The columns of this <see cref="StatisticRow"/>.
        /// </summary>
        [ItemNotNull]
        public StatisticItem[] Columns;

        public Drawable CreateDrawableStatisticRow() => new GridContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Content = new[]
            {
                Columns?.Select(c => new StatisticContainer(c)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }).Cast<Drawable>().ToArray()
            },
            ColumnDimensions = Enumerable.Range(0, Columns?.Length ?? 0)
                                         .Select(i => Columns[i].Dimension ?? new Dimension()).ToArray(),
            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
        };
    }
}
