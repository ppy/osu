// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.Placeholders;
using osu.Game.Overlays;
using osu.Game.Overlays.Volume;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    [Cached]
    public abstract partial class ResultsScreen : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>
    {
        protected const float BACKGROUND_BLUR = 20;
        private static readonly float screen_height = 768 - TwoLayerButton.SIZE_EXTENDED.Y;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public override bool? AllowGlobalTrackControl => true;

        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.UserTriggered;

        public readonly Bindable<ScoreInfo?> SelectedScore = new Bindable<ScoreInfo?>();

        public readonly ScoreInfo? Score;

        protected ScorePanelList ScorePanelList { get; private set; } = null!;

        protected VerticalScrollContainer VerticalScrollContent { get; private set; } = null!;

        [Resolved]
        private Player? player { get; set; }

        private bool skipExitTransition;

        protected StatisticsPanel StatisticsPanel { get; private set; } = null!;

        private Drawable bottomPanel = null!;
        private Container<ScorePanel> detachedPanelContainer = null!;

        private Task lastFetchTask = Task.CompletedTask;

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
        /// after clicking the score panel associated with the <see cref="Score"/> being presented.
        /// Requires <see cref="Score"/> to be present.
        /// </summary>
        public bool ShowUserStatistics { get; init; }

        private Sample? popInSample;

        protected ResultsScreen(ScoreInfo? score)
        {
            Score = score;

            SelectedScore.Value = score;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            FillFlowContainer buttons;

            popInSample = audio.Samples.Get(@"UI/overlay-pop-in");

            InternalChild = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new GridContainer
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
                                        new GlobalScrollAdjustsVolume(),
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
                                    },
                                }
                            }
                        }
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize)
                    }
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

            bool allowHotkeyRetry = false;

            if (AllowWatchingReplay)
            {
                buttons.Add(new ReplayDownloadButton(SelectedScore.Value)
                {
                    Score = { BindTarget = SelectedScore },
                    Width = 300
                });

                // for simplicity, only allow this when coming from a replay player where we know the replay is ready to be played.
                //
                // if we show it in all cases, consider the case where a user comes from song select and potentially has to download
                // the replay before it can be played back. it wouldn't flow well with the quick retry in such a case.
                allowHotkeyRetry = player is ReplayPlayer;
            }

            if (player != null && AllowRetry)
            {
                buttons.Add(new RetryButton { Width = 300 });
                allowHotkeyRetry = true;
            }

            if (allowHotkeyRetry)
            {
                AddInternal(new HotkeyRetryOverlay
                {
                    Action = () =>
                    {
                        if (!this.IsCurrentScreen()) return;

                        skipExitTransition = true;
                        player?.Restart(true);
                    },
                });
            }

            if (Score?.BeatmapInfo != null)
                buttons.Add(new CollectionButton(Score.BeatmapInfo));

            if (Score?.BeatmapInfo?.BeatmapSet != null && Score.BeatmapInfo.BeatmapSet.OnlineID > 0)
                buttons.Add(new FavouriteButton(Score.BeatmapInfo.BeatmapSet));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            StatisticsPanel.State.BindValueChanged(onStatisticsStateChanged, true);

            fetchScores(null);
        }

        protected override void Update()
        {
            base.Update();

            if (ScorePanelList.IsScrolledToStart)
                fetchScores(-1);
            else if (ScorePanelList.IsScrolledToEnd)
                fetchScores(1);
        }

        #region Applause

        private PoolableSkinnableSample? rankApplauseSound;

        public void PlayApplause(ScoreRank rank)
        {
            const double applause_volume = 0.8f;

            if (!this.IsCurrentScreen())
                return;

            rankApplauseSound?.Dispose();

            var applauseSamples = new List<string>();

            if (rank >= ScoreRank.B)
                // when rank is B or higher, play legacy applause sample on legacy skins.
                applauseSamples.Insert(0, @"applause");

            switch (rank)
            {
                default:
                case ScoreRank.D:
                    applauseSamples.Add(@"Results/applause-d");
                    break;

                case ScoreRank.C:
                    applauseSamples.Add(@"Results/applause-c");
                    break;

                case ScoreRank.B:
                    applauseSamples.Add(@"Results/applause-b");
                    break;

                case ScoreRank.A:
                    applauseSamples.Add(@"Results/applause-a");
                    break;

                case ScoreRank.S:
                case ScoreRank.SH:
                case ScoreRank.X:
                case ScoreRank.XH:
                    applauseSamples.Add(@"Results/applause-s");
                    break;
            }

            LoadComponentAsync(rankApplauseSound = new PoolableSkinnableSample(new SampleInfo(applauseSamples.ToArray())), s =>
            {
                if (!this.IsCurrentScreen() || s != rankApplauseSound)
                    return;

                AddInternal(rankApplauseSound);

                rankApplauseSound.VolumeTo(applause_volume);
                rankApplauseSound.Play();
            });
        }

        #endregion

        /// <summary>
        /// Fetches the next page of scores in the given direction.
        /// </summary>
        /// <param name="direction">The direction, or <c>null</c> to fetch any scores.</param>
        private void fetchScores(int? direction)
        {
            Debug.Assert(direction == null || direction == -1 || direction == 1);

            if (!lastFetchTask.IsCompleted)
                return;

            lastFetchTask = Task.Run(async () =>
            {
                ScoreInfo[] scores;

                switch (direction)
                {
                    default:
                        scores = await FetchScores().ConfigureAwait(false);
                        break;

                    case -1:
                    case 1:
                        scores = await FetchNextPage(direction.Value).ConfigureAwait(false);
                        break;
                }

                await addScores(scores).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Performs a fetch/refresh of scores to be displayed.
        /// </summary>
        protected virtual Task<ScoreInfo[]> FetchScores() => Task.FromResult<ScoreInfo[]>([]);

        /// <summary>
        /// Performs a fetch of the next page of scores. This is invoked every frame.
        /// </summary>
        /// <param name="direction">The fetch direction. -1 to fetch scores greater than the current start of the list, and 1 to fetch scores lower than the current end of the list.</param>
        protected virtual Task<ScoreInfo[]> FetchNextPage(int direction) => Task.FromResult<ScoreInfo[]>([]);

        /// <summary>
        /// Creates the <see cref="Statistics.StatisticsPanel"/> to be used to display extended information about scores.
        /// </summary>
        private StatisticsPanel createStatisticsPanel()
        {
            return ShowUserStatistics && Score != null
                ? new UserStatisticsPanel(Score)
                : new StatisticsPanel();
        }

        private Task addScores(ScoreInfo[] scores)
        {
            var tcs = new TaskCompletionSource();

            Schedule(() =>
            {
                foreach (var s in scores)
                {
                    var panel = ScorePanelList.AddScore(s);
                    if (detachedPanel != null)
                        panel.Alpha = 0;
                }

                // allow a frame for scroll container to adjust its dimensions with the added scores before fetching again.
                Schedule(() => tcs.SetResult());

                if (ScorePanelList.IsEmpty)
                {
                    // This can happen if for example a beatmap that is part of a playlist hasn't been played yet.
                    VerticalScrollContent.Add(new MessagePlaceholder(LeaderboardStrings.NoRecordsYet));
                }

                OnScoresAdded(scores);
            });

            return tcs.Task;
        }

        /// <summary>
        /// Invoked after online scores are fetched and added to the list.
        /// </summary>
        /// <param name="scores">The scores that were added.</param>
        protected virtual void OnScoresAdded(ScoreInfo[] scores)
        {
        }

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

            if (!skipExitTransition)
                this.FadeOut(100);

            rankApplauseSound?.Stop();
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

        private ScorePanel? detachedPanel;

        private void onStatisticsStateChanged(ValueChangedEvent<Visibility> state)
        {
            if (state.NewValue == Visibility.Visible)
            {
                Debug.Assert(SelectedScore.Value != null);
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
                case GlobalAction.QuickExit:
                    if (this.IsCurrentScreen())
                    {
                        this.Exit();
                        return true;
                    }

                    break;

                case GlobalAction.Select:
                    if (SelectedScore.Value != null)
                        StatisticsPanel.ToggleVisibility();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // Match stable behaviour of only alt-scroll adjusting volume.
            // This is the same behaviour as the song selection screen.
            if (!e.CurrentState.Keyboard.AltPressed)
                return true;

            return base.OnScroll(e);
        }

        protected partial class VerticalScrollContainer : OsuScrollContainer
        {
            protected override Container<Drawable> Content => content;

            private readonly Container content;

            protected override bool OnScroll(ScrollEvent e) => !e.ControlPressed && !e.AltPressed && !e.ShiftPressed && !e.SuperPressed;

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
