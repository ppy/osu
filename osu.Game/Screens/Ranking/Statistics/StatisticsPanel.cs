// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticsPanel : CompositeDrawable
    {
        public StatisticsPanel(ScoreInfo score)
        {
            // Todo: Not correct.
            RelativeSizeAxes = Axes.Both;

            FillFlowContainer statisticRows;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#333")
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = ScorePanel.EXPANDED_WIDTH + 30 + 50,
                        Right = 50
                    },
                    Child = statisticRows = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(30, 15),
                    }
                }
            };

            foreach (var s in score.Ruleset.CreateInstance().CreateStatistics(score))
            {
                statisticRows.Add(new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Content = new[] { s.Content },
                    ColumnDimensions = s.ColumnDimensions,
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                });
            }
        }
    }
}
