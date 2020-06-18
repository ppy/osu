// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticsPanel : VisibilityContainer
    {
        public const float SIDE_PADDING = 30;

        protected override bool StartHidden => true;

        public StatisticsPanel(ScoreInfo score)
        {
            // Todo: Not correct.
            RelativeSizeAxes = Axes.Both;

            FillFlowContainer statisticRows;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Left = ScorePanel.EXPANDED_WIDTH + SIDE_PADDING * 3,
                    Right = SIDE_PADDING,
                    Top = SIDE_PADDING,
                    Bottom = 50 // Approximate padding to the bottom of the score panel.
                },
                Child = statisticRows = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(30, 15),
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

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();
    }
}
