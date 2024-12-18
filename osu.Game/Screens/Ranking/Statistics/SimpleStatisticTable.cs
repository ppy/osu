// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Represents a table with simple statistics (ones that only need textual display).
    /// Richer visualisations should be done with <see cref="StatisticItem"/>s.
    /// </summary>
    public partial class SimpleStatisticTable : CompositeDrawable
    {
        private readonly SimpleStatisticItem[] items;
        private readonly int columnCount;

        private FillFlowContainer[] columns = null!;

        /// <summary>
        /// Creates a statistic row for the supplied <see cref="SimpleStatisticItem"/>s.
        /// </summary>
        /// <param name="columnCount">The number of columns to layout the <paramref name="items"/> into.</param>
        /// <param name="items">The <see cref="SimpleStatisticItem"/>s to display in this row.</param>
        public SimpleStatisticTable(int columnCount, IEnumerable<SimpleStatisticItem> items)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columnCount);

            this.columnCount = columnCount;
            this.items = items.ToArray();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            columns = new FillFlowContainer[columnCount];

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                ColumnDimensions = createColumnDimensions().ToArray(),
                Content = new[] { createColumns().ToArray() }
            };

            for (int i = 0; i < items.Length; ++i)
                columns[i % columnCount].Add(items[i]);
        }

        private IEnumerable<Dimension> createColumnDimensions()
        {
            for (int column = 0; column < columnCount; ++column)
            {
                if (column > 0)
                    yield return new Dimension(GridSizeMode.Absolute, 30);

                yield return new Dimension();
            }
        }

        private IEnumerable<Drawable> createColumns()
        {
            for (int column = 0; column < columnCount; ++column)
            {
                if (column > 0)
                {
                    yield return new Spacer
                    {
                        Alpha = items.Length > column ? 1 : 0
                    };
                }

                yield return columns[column] = createColumn();
            }
        }

        private FillFlowContainer createColumn() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical
        };

        public partial class Spacer : CompositeDrawable
        {
            public Spacer()
            {
                RelativeSizeAxes = Axes.Both;

                InternalChild = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 3,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    CornerRadius = 2,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4Extensions.FromHex("#222")
                    }
                };
            }
        }
    }
}
