// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Placeholders;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    public class StatisticsPanel : VisibilityContainer
    {
        public const float SIDE_PADDING = 30;

        public readonly Bindable<ScoreInfo> Score = new Bindable<ScoreInfo>();

        protected override bool StartHidden => true;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

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
            loadCancellation = null;

            foreach (var child in content)
                child.FadeOut(150).Expire();

            spinner.Hide();

            var newScore = score.NewValue;

            if (newScore == null)
                return;

            if (newScore.HitEvents == null || newScore.HitEvents.Count == 0)
            {
                content.Add(new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new MessagePlaceholder("Extended statistics are only available after watching a replay!"),
                        new ReplayDownloadButton(newScore)
                        {
                            Scale = new Vector2(1.5f),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                });
            }
            else
            {
                spinner.Show();

                var localCancellationSource = loadCancellation = new CancellationTokenSource();
                IBeatmap playableBeatmap = null;

                // Todo: The placement of this is temporary. Eventually we'll both generate the playable beatmap _and_ run through it in a background task to generate the hit events.
                Task.Run(() =>
                {
                    playableBeatmap = beatmapManager.GetWorkingBeatmap(newScore.BeatmapInfo).GetPlayableBeatmap(newScore.Ruleset, newScore.Mods ?? Array.Empty<Mod>());
                }, loadCancellation.Token).ContinueWith(t => Schedule(() =>
                {
                    var rows = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(30, 15),
                        Alpha = 0
                    };

                    foreach (var row in newScore.Ruleset.CreateInstance().CreateStatisticsForScore(newScore, playableBeatmap))
                    {
                        rows.Add(new GridContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Content = new[]
                            {
                                row.Columns?.Select(c => new StatisticContainer(c)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                }).Cast<Drawable>().ToArray()
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
                        d.FadeIn(250, Easing.OutQuint);
                    }, localCancellationSource.Token);
                }), localCancellationSource.Token);
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            ToggleVisibility();
            return true;
        }

        protected override void PopIn() => this.FadeIn(150, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(150, Easing.OutQuint);

        protected override void Dispose(bool isDisposing)
        {
            loadCancellation?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
