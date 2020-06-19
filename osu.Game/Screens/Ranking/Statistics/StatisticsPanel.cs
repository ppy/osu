// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
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
        private readonly LoadingSpinner spinner;

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
                Children = new Drawable[]
                {
                    content = new Container { RelativeSizeAxes = Axes.Both },
                    spinner = new LoadingSpinner()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Score.BindValueChanged(populateStatistics, true);
        }

        private CancellationTokenSource loadCancellation;

        private void populateStatistics(ValueChangedEvent<ScoreInfo> score)
        {
            loadCancellation?.Cancel();

            foreach (var child in content)
                child.FadeOut(150).Expire();

            var newScore = score.NewValue;

            if (newScore.HitEvents == null || newScore.HitEvents.Count == 0)
                content.Add(new MessagePlaceholder("Score has no statistics :("));
            else
            {
                spinner.Show();

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
                        Content = new[]
                        {
                            row.Columns?.Select(c => new StatisticContainer(c)).Cast<Drawable>().ToArray()
                        },
                        ColumnDimensions = Enumerable.Range(0, row.Columns?.Length ?? 0)
                                                     .Select(i => row.Columns[i].Dimension ?? new Dimension()).ToArray(),
                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) }
                    });
                }

                LoadComponentAsync(rows, d =>
                {
                    if (Score.Value != newScore)
                        return;

                    spinner.Hide();
                    content.Add(d);
                }, (loadCancellation = new CancellationTokenSource()).Token);
            }
        }

        protected override void PopIn() => this.FadeIn(150, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(150, Easing.OutQuint);
    }
}
