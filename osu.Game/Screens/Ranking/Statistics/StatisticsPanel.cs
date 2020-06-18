// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Placeholders;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticsPanel : VisibilityContainer
    {
        public const float SIDE_PADDING = 30;

        public readonly Bindable<ScoreInfo> Score = new Bindable<ScoreInfo>();

        protected override bool StartHidden => true;

        private readonly Container content;

        public StatisticsPanel()
        {
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
                Child = content = new Container { RelativeSizeAxes = Axes.Both },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Score.BindValueChanged(populateStatistics, true);
        }

        private void populateStatistics(ValueChangedEvent<ScoreInfo> score)
        {
            foreach (var child in content)
                child.FadeOut(150).Expire();

            var newScore = score.NewValue;

            if (newScore.HitEvents == null || newScore.HitEvents.Count == 0)
                content.Add(new MessagePlaceholder("Score has no statistics :("));
            else
            {
                var rows = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(30, 15),
                };

                foreach (var row in newScore.Ruleset.CreateInstance().CreateStatistics(newScore))
                {
                    rows.Add(new GridContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Content = new[] { row.Content },
                        ColumnDimensions = row.ColumnDimensions,
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                    });
                }

                content.Add(rows);
            }
        }

        protected override void PopIn() => this.FadeIn();

        protected override void PopOut() => this.FadeOut();
    }
}
