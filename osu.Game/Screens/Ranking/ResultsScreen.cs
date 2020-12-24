// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Online.API;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public abstract class ResultsScreen : OsuScreen, IKeyBindingHandler<GlobalAction>
    {
        protected const float BACKGROUND_BLUR = 20;
        private static readonly float screen_height = 768 - TwoLayerButton.SIZE_EXTENDED.Y;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // Temporary for now to stop dual transitions. Should respect the current toolbar mode, but there's no way to do so currently.
        public override bool HideOverlaysOnEnter => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        public readonly Bindable<ScoreInfo> SelectedScore = new Bindable<ScoreInfo>();

        public readonly ScoreInfo Score;

        protected ScorePanelList ScorePanelList { get; private set; }

        [Resolved(CanBeNull = true)]
        private Player player { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private StatisticsPanel statisticsPanel;
        private Drawable bottomPanel;
        private Container<ScorePanel> detachedPanelContainer;

        private bool fetchedInitialScores;
        private APIRequest nextPageRequest;

        private readonly bool allowRetry;
        private readonly bool allowWatchingReplay;

        protected ResultsScreen(ScoreInfo score, bool allowRetry, bool allowWatchingReplay = true)
        {
            Score = score;
            this.allowRetry = allowRetry;
            this.allowWatchingReplay = allowWatchingReplay;

            SelectedScore.Value = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            FillFlowContainer buttons;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new VerticalScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ScrollbarVisible = false,
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    statisticsPanel = new StatisticsPanel
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Score = { BindTarget = SelectedScore }
                                    },
                                    ScorePanelList = new ScorePanelList
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        SelectedScore = { BindTarget = SelectedScore },
                                        PostExpandAction = () => statisticsPanel.ToggleVisibility()
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
                bool shouldFlair = player != null && !Score.Mods.Any(m => m is ModAutoplay);

                ScorePanelList.AddScore(Score, shouldFlair);
            }

            if (allowWatchingReplay)
            {
                buttons.Add(new ReplayDownloadButton(null)
                {
                    Score = { BindTarget = SelectedScore },
                    Width = 300
                });
            }

            if (player != null && allowRetry)
            {
                buttons.Add(new RetryButton { Width = 300 });

                AddInternal(new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        player?.Restart();
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

            statisticsPanel.State.BindValueChanged(onStatisticsStateChanged, true);
        }

        protected override void Update()
        {
            base.Update();

            if (fetchedInitialScores && nextPageRequest == null)
            {
                if (ScorePanelList.IsScrolledToStart)
                    nextPageRequest = FetchNextPage(-1, fetchScoresCallback);
                else if (ScorePanelList.IsScrolledToEnd)
                    nextPageRequest = FetchNextPage(1, fetchScoresCallback);

                if (nextPageRequest != null)
                {
                    // Scheduled after children to give the list a chance to update its scroll position and not potentially trigger a second request too early.
                    nextPageRequest.Success += () => ScheduleAfterChildren(() => nextPageRequest = null);
                    nextPageRequest.Failure += _ => ScheduleAfterChildren(() => nextPageRequest = null);

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

        private void fetchScoresCallback(IEnumerable<ScoreInfo> scores) => Schedule(() =>
        {
            foreach (var s in scores)
                addScore(s);

            fetchedInitialScores = true;
        });

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            ((BackgroundScreenBeatmap)Background).BlurAmount.Value = BACKGROUND_BLUR;

            Background.FadeTo(0.5f, 250);
            bottomPanel.FadeTo(1, 250);
        }

        public override bool OnExiting(IScreen next)
        {
            Background.FadeTo(1, 250);

            return base.OnExiting(next);
        }

        public override bool OnBackButton()
        {
            if (statisticsPanel.State.Value == Visibility.Visible)
            {
                statisticsPanel.Hide();
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
                var origLocation = detachedPanelContainer.ToLocalSpace(screenSpacePos).X;
                expandedPanel.MoveToX(origLocation)
                             .Then()
                             .MoveToX(StatisticsPanel.SIDE_PADDING, 150, Easing.OutQuint);

                // Hide contracted panels.
                foreach (var contracted in ScorePanelList.GetScorePanels().Where(p => p.State == PanelState.Contracted))
                    contracted.FadeOut(150, Easing.OutQuint);
                ScorePanelList.HandleInput = false;

                // Dim background.
                Background.FadeTo(0.1f, 150);

                detachedPanel = expandedPanel;
            }
            else if (detachedPanel != null)
            {
                var screenSpacePos = detachedPanel.ScreenSpaceDrawQuad.TopLeft;

                // Remove from the local container and re-attach.
                detachedPanelContainer.Remove(detachedPanel);
                ScorePanelList.Attach(detachedPanel);

                // Move into its original location in the attached container first, then to the final location.
                var origLocation = detachedPanel.Parent.ToLocalSpace(screenSpacePos);
                detachedPanel.MoveTo(origLocation)
                             .Then()
                             .MoveTo(new Vector2(0, origLocation.Y), 150, Easing.OutQuint);

                // Show contracted panels.
                foreach (var contracted in ScorePanelList.GetScorePanels().Where(p => p.State == PanelState.Contracted))
                    contracted.FadeIn(150, Easing.OutQuint);
                ScorePanelList.HandleInput = true;

                // Un-dim background.
                Background.FadeTo(0.5f, 150);

                detachedPanel = null;
            }
        }

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Select:
                    statisticsPanel.ToggleVisibility();
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
        }

        private class VerticalScrollContainer : OsuScrollContainer
        {
            protected override Container<Drawable> Content => content;

            private readonly Container content;

            public VerticalScrollContainer()
            {
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
