// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Placeholders;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class StatisticsPanel : VisibilityContainer
    {
        public const float SIDE_PADDING = 30;

        public readonly Bindable<ScoreInfo?> Score = new Bindable<ScoreInfo?>();

        protected override bool StartHidden => true;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        private readonly Container content;
        private readonly LoadingSpinner spinner;

        private bool wasOpened;
        private Sample? popInSample;
        private Sample? popOutSample;
        private CancellationTokenSource? loadCancellation;

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
        private void load(AudioManager audio)
        {
            Score.BindValueChanged(populateStatistics, true);

            popInSample = audio.Samples.Get(@"Results/statistics-panel-pop-in");
            popOutSample = audio.Samples.Get(@"Results/statistics-panel-pop-out");
        }

        private void populateStatistics(ValueChangedEvent<ScoreInfo?> score)
        {
            loadCancellation?.Cancel();
            loadCancellation = null;

            foreach (var child in content)
                child.FadeOut(150).Expire();

            spinner.Hide();

            var newScore = score.NewValue;

            if (newScore == null)
                return;

            spinner.Show();

            var localCancellationSource = loadCancellation = new CancellationTokenSource();

            var workingBeatmap = beatmapManager.GetWorkingBeatmap(newScore.BeatmapInfo);

            // Todo: The placement of this is temporary. Eventually we'll both generate the playable beatmap _and_ run through it in a background task to generate the hit events.
            Task.Run(() => workingBeatmap.GetPlayableBeatmap(newScore.Ruleset, newScore.Mods), loadCancellation.Token).ContinueWith(task => Schedule(() =>
            {
                bool hitEventsAvailable = newScore.HitEvents.Count != 0;
                Container<Drawable> container;

                var statisticItems = CreateStatisticItems(newScore, task.GetResultSafely());

                if (!hitEventsAvailable && statisticItems.All(c => c.RequiresHitEvents))
                {
                    container = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
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
                    };
                }
                else
                {
                    FillFlowContainer flow;
                    container = new OsuScrollContainer(Direction.Vertical)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Masking = false,
                        ScrollbarOverlapsContent = false,
                        Alpha = 0,
                        Children = new[]
                        {
                            flow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Spacing = new Vector2(30, 15),
                                Direction = FillDirection.Full,
                            }
                        }
                    };

                    bool anyRequiredHitEvents = false;

                    foreach (var item in statisticItems)
                    {
                        if (!hitEventsAvailable && item.RequiresHitEvents)
                        {
                            anyRequiredHitEvents = true;
                            continue;
                        }

                        flow.Add(new StatisticItemContainer(item)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        });
                    }

                    if (anyRequiredHitEvents)
                    {
                        flow.Add(new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new MessagePlaceholder("More statistics available after watching a replay!"),
                                new ReplayDownloadButton(newScore)
                                {
                                    Scale = new Vector2(1.5f),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                            }
                        });
                    }
                }

                LoadComponentAsync(container, d =>
                {
                    if (Score.Value?.Equals(newScore) != true)
                        return;

                    spinner.Hide();
                    content.Add(d);
                    d.FadeIn(250, Easing.OutQuint);
                }, localCancellationSource.Token);
            }), localCancellationSource.Token);
        }

        /// <summary>
        /// Creates the <see cref="StatisticItem"/>s to be displayed in this panel for a given <paramref name="newScore"/>.
        /// </summary>
        /// <param name="newScore">The score to create the rows for.</param>
        /// <param name="playableBeatmap">The beatmap on which the score was set.</param>
        protected virtual ICollection<StatisticItem> CreateStatisticItems(ScoreInfo newScore, IBeatmap playableBeatmap)
            => newScore.Ruleset.CreateInstance().CreateStatisticsForScore(newScore, playableBeatmap);

        protected override bool OnClick(ClickEvent e)
        {
            ToggleVisibility();
            return true;
        }

        protected override void PopIn()
        {
            this.FadeIn(350, Easing.OutQuint);

            popInSample?.Play();
            wasOpened = true;
        }

        protected override void PopOut()
        {
            this.FadeOut(250, Easing.OutQuint);

            if (wasOpened)
                popOutSample?.Play();
        }

        protected override void Dispose(bool isDisposing)
        {
            loadCancellation?.Cancel();

            base.Dispose(isDisposing);
        }
    }
}
