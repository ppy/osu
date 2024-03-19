// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public abstract partial class ResultsScreen : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>
    {
        protected const float BACKGROUND_BLUR = 20;
        private static readonly float screen_height = 768 - TwoLayerButton.SIZE_EXTENDED.Y;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? AllowGlobalTrackControl => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        public readonly Bindable<ScoreInfo> SelectedScore = new Bindable<ScoreInfo>();

        [CanBeNull]
        public readonly ScoreInfo Score;

        protected ScorePanelList ScorePanelList { get; private set; }

        protected VerticalScrollContainer VerticalScrollContent { get; private set; }

        [Resolved(CanBeNull = true)]
        private Player player { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        protected StatisticsPanel StatisticsPanel { get; private set; }

        private Drawable bottomPanel;
        private Container<ScorePanel> detachedPanelContainer;

        private bool lastFetchCompleted;

        /// <summary>
        /// Whether the user can retry the beatmap from the results screen.
        /// </summary>
        public bool AllowRetry { get; init; }

        /// <summary>
        /// Whether the user can watch the replay of the completed play from the results screen.
        /// </summary>
        public bool AllowWatchingReplay { get; init; } = true;

        /// <summary>
        /// Whether the user's personal statistics should be shown on the extended statistics panel
        /// after clicking the score panel associated with the <see cref="ResultsScreen.Score"/> being presented.
        /// Requires <see cref="Score"/> to be present.
        /// </summary>
        public bool ShowUserStatistics { get; init; }

        private Sample popInSample;

        protected ResultsScreen([CanBeNull] ScoreInfo score)
        {
            Score = score;

            SelectedScore.Value = score;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            FillFlowContainer buttons;

            popInSample = audio.Samples.Get(@"UI/overlay-pop-in");

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        VerticalScrollContent = new VerticalScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarVisible = false,
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    StatisticsPanel = createStatisticsPanel().With(panel =>
                                    {
                                        panel.RelativeSizeAxes = Axes.Both;
                                        panel.Score.BindTarget = SelectedScore;
                                    }),
                                    ScorePanelList = new ScorePanelList
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        SelectedScore = { BindTarget = SelectedScore },
                                        PostExpandAction = () => StatisticsPanel.ToggleVisibility()
                                    },
                                    detachedPanelContainer = new Container<ScorePanel>
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                }
                            }
                        },
                    },
                    new[]
                    {
                        bottomPanel = new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = TwoLayerButton.SIZE_EXTENDED.Y,
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex("#333")
                                },
                                buttons = new FillFlowContainer
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(5),
                                    Direction = FillDirection.Horizontal
                                }
                            }
                        }
                    }
                },
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize)
                }
            };

            if (Score != null)
            {
                // only show flair / animation when arriving after watching a play that isn't autoplay.
                bool shouldFlair = player != null && !Score.User.IsBot;

                ScorePanelList.AddScore(Score, shouldFlair);
                // this is mostly for medal display.
                // we don't want the medal animation to trample on the results screen animation, so we (ab)use `OverlayActivationMode`
                // to give the results screen enough time to play the animation out before the medals can be shown.
                Scheduler.AddDelayed(() => OverlayActivationMode.Value = OverlayActivation.All, shouldFlair ? AccuracyCircle.TOTAL_DURATION + 1000 : 0);
            }

            if (AllowWatchingReplay)
            {
                buttons.Add(new ReplayDownloadButton(SelectedScore.Value)
                {
                    Score = { BindTarget = SelectedScore },
                    Width = 300
                });
            }

            if (player != null && AllowRetry)
            {
                buttons.Add(new RetryButton { Width = 300 });

                AddInternal(new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        player?.Restart(true);
                    },
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var req = FetchScores(fetchScoresCallback);

            if (req != null)
                api.Queue(req);

            StatisticsPanel.State.BindValueChanged(onStatisticsStateChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            if (lastFetchCompleted)
            {
                APIRequest nextPageRequest = null;

                if (ScorePanelList.IsScrolledToStart)
                    nextPageRequest = FetchNextPage(-1, fetchScoresCallback);
                else if (ScorePanelList.IsScrolledToEnd)
                    nextPageRequest = FetchNextPage(1, fetchScoresCallback);

                if (nextPageRequest != null)
                {
                    lastFetchCompleted = false;
                    api.Queue(nextPageRequest);
                }
            }
        }

        /// <summary>
        /// Performs a fetch/refresh of scores to be displayed.
        /// </summary>
        /// <param name="scoresCallback">A callback which should be called when fetching is completed. Scheduling is not required.</param>
        /// <returns>An <see cref="APIRequest"/> responsible for the fetch operation. This will be queued and performed automatically.</returns>
        protected virtual APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback) => null;

        /// <summary>
        /// Performs a fetch of the next page of scores. This is invoked every frame until a non-null <see cref="APIRequest"/> is returned.
        /// </summary>
        /// <param name="direction">The fetch direction. -1 to fetch scores greater than the current start of the list, and 1 to fetch scores lower than the current end of the list.</param>
        /// <param name="scoresCallback">A callback which should be called when fetching is completed. Scheduling is not required.</param>
        /// <returns>An <see cref="APIRequest"/> responsible for the fetch operation. This will be queued and performed automatically.</returns>
        protected virtual APIRequest FetchNextPage(int direction, Action<IEnumerable<ScoreInfo>> scoresCallback) => null;

        /// <summary>
        /// Creates the <see cref="Statistics.StatisticsPanel"/> to be used to display extended information about scores.
        /// </summary>
        private StatisticsPanel createStatisticsPanel()
        {
            return ShowUserStatistics && Score != null
                ? new UserStatisticsPanel(Score)
                : new StatisticsPanel();
        }

        private void fetchScoresCallback(IEnumerable<ScoreInfo> scores) => Schedule(() =>
        {
            foreach (var s in scores)
                addScore(s);

            lastFetchCompleted = true;

            if (ScorePanelList.IsEmpty)
            {
                // This can happen if for example a beatmap that is part of a playlist hasn't been played yet.
                VerticalScrollContent.Add(new MessagePlaceholder(LeaderboardStrings.NoRecordsYet));
            }
        });

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            ApplyToBackground(b =>
            {
                b.BlurAmount.Value = BACKGROUND_BLUR;
                b.FadeColour(OsuColour.Gray(0.5f), 250);
            });

            bottomPanel.FadeTo(1, 250);

            popInSample?.Play();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            // This is a stop-gap safety against components holding references to gameplay after exiting the gameplay flow.
            // Right now, HitEvents are only used up to the results screen. If this changes in the future we need to remove
            // HitObject references from HitEvent.
            Score?.HitEvents.Clear();

            this.FadeOut(100);
            return false;
        }

        public override bool OnBackButton()
        {
            if (StatisticsPanel.State.Value == Visibility.Visible)
            {
                StatisticsPanel.Hide();
                return true;
            }

            return false;
        }

        private void addScore(ScoreInfo score)
        {
            var panel = ScorePanelList.AddScore(score);

            if (detachedPanel != null)
                panel.Alpha = 0;
        }

        private ScorePanel detachedPanel;

        private void onStatisticsStateChanged(ValueChangedEvent<Visibility> state)
        {
            if (state.NewValue == Visibility.Visible)
            {
                // Detach the panel in its original location, and move into the desired location in the local container.
                var expandedPanel = ScorePanelList.GetPanelForScore(SelectedScore.Value);
                var screenSpacePos = expandedPanel.ScreenSpaceDrawQuad.TopLeft;

                // Detach and move into the local container.
                ScorePanelList.Detach(expandedPanel);
                detachedPanelContainer.Add(expandedPanel);

                // Move into its original location in the local container first, then to the final location.
                float origLocation = detachedPanelContainer.ToLocalSpace(screenSpacePos).X;
                expandedPanel.MoveToX(origLocation)
                             .Then()
                             .MoveToX(StatisticsPanel.SIDE_PADDING, 400, Easing.OutElasticQuarter);

                // Hide contracted panels.
                foreach (var contracted in ScorePanelList.GetScorePanels().Where(p => p.State == PanelState.Contracted))
                    contracted.FadeOut(150, Easing.OutQuint);
                ScorePanelList.HandleInput = false;

                // Dim background.
                ApplyToBackground(b => b.FadeColour(OsuColour.Gray(0.4f), 400, Easing.OutQuint));

                detachedPanel = expandedPanel;
            }
            else if (detachedPanel != null)
            {
                var screenSpacePos = detachedPanel.ScreenSpaceDrawQuad.TopLeft;

                // Remove from the local container and re-attach.
                detachedPanelContainer.Remove(detachedPanel, false);
                ScorePanelList.Attach(detachedPanel);

                // Move into its original location in the attached container first, then to the final location.
                float origLocation = detachedPanel.Parent!.ToLocalSpace(screenSpacePos).X;
                detachedPanel.MoveToX(origLocation)
                             .Then()
                             .MoveToX(0, 250, Easing.OutElasticQuarter);

                // Show contracted panels.
                foreach (var contracted in ScorePanelList.GetScorePanels().Where(p => p.State == PanelState.Contracted))
                    contracted.FadeIn(150, Easing.OutQuint);
                ScorePanelList.HandleInput = true;

                // Un-dim background.
                ApplyToBackground(b => b.FadeColour(OsuColour.Gray(0.5f), 250, Easing.OutQuint));

                detachedPanel = null;
            }
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    StatisticsPanel.ToggleVisibility();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected partial class VerticalScrollContainer : OsuScrollContainer
        {
            protected override Container<Drawable> Content => content;

            private readonly Container content;

            public VerticalScrollContainer()
            {
                Masking = false;

                base.Content.Add(content = new Container { RelativeSizeAxes = Axes.X });
            }

            protected override void Update()
            {
                base.Update();
                content.Height = Math.Max(screen_height, DrawHeight);
            }
        }
    }
}
