// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Scoring;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class ScoreTable : CompositeDrawable
    {
        private readonly ScoresGrid scoresGrid;
        private readonly FillFlowContainer backgroundFlow;

        public ScoreTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                backgroundFlow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding { Top = 25 }
                },
                scoresGrid = new ScoresGrid
                {
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 40),
                        new Dimension(GridSizeMode.Absolute, 70),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed, minSize: 150),
                        new Dimension(GridSizeMode.Distributed, minSize: 70, maxSize: 90),
                        new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                        new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                        new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                        new Dimension(GridSizeMode.Distributed, minSize: 50, maxSize: 70),
                        new Dimension(GridSizeMode.Distributed, minSize: 40, maxSize: 70),
                        new Dimension(GridSizeMode.AutoSize),
                    }
                }
            };
        }

        public IReadOnlyList<ScoreInfo> Scores
        {
            set
            {
                scoresGrid.Content = new Drawable[0][];
                backgroundFlow.Clear();

                if (value == null || !value.Any())
                    return;

                var content = new List<Drawable[]>
                {
                    new ScoreTableHeaderRow(value.First()).CreateDrawables().ToArray()
                };

                for (int i = 0; i < value.Count; i++)
                {
                    content.Add(new ScoreTableScoreRow(i, value[i]).CreateDrawables().ToArray());
                    backgroundFlow.Add(new ScoreTableRowBackground(i));
                }

                scoresGrid.Content = content.ToArray();
            }
        }

        private class ScoresGrid : GridContainer
        {
            public ScoresGrid()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public Drawable[][] Content
            {
                get => base.Content;
                set
                {
                    base.Content = value;

                    RowDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.Absolute, 25), value.Length).ToArray();
                }
            }
        }
    }
}
