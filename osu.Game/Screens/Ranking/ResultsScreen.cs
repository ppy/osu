// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking.Statistics;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public abstract class ResultsScreen : OsuScreen
    {
        protected const float BACKGROUND_BLUR = 20;
        private static readonly float screen_height = 768 - TwoLayerButton.SIZE_EXTENDED.Y;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // Temporary for now to stop dual transitions. Should respect the current toolbar mode, but there's no way to do so currently.
        public override bool HideOverlaysOnEnter => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        public readonly Bindable<ScoreInfo> SelectedScore = new Bindable<ScoreInfo>();

        public readonly ScoreInfo Score;
        private readonly bool allowRetry;

        [Resolved(CanBeNull = true)]
        private Player player { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        private StatisticsPanel statisticsPanel;
        private Drawable bottomPanel;
        private ScorePanelList scorePanelList;

        protected ResultsScreen(ScoreInfo score, bool allowRetry = true)
        {
            Score = score;
            this.allowRetry = allowRetry;

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
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ScrollbarVisible = false,
                                    Child = new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = screen_height,
                                        Children = new Drawable[]
                                        {
                                            scorePanelList = new ScorePanelList
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                SelectedScore = { BindTarget = SelectedScore },
                                                PostExpandAction = onExpandedPanelClicked
                                            },
                                            statisticsPanel = new StatisticsPanel
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Score = { BindTarget = SelectedScore }
                                            }
                                        }
                                    }
                                },
                            }
                        }
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
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new ReplayDownloadButton(null)
                                        {
                                            Score = { BindTarget = SelectedScore },
                                            Width = 300
                                        },
                                    }
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
                scorePanelList.AddScore(Score);

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

            var req = FetchScores(scores => Schedule(() =>
            {
                foreach (var s in scores)
                    scorePanelList.AddScore(s);
            }));

            if (req != null)
                api.Queue(req);
        }

        /// <summary>
        /// Performs a fetch/refresh of scores to be displayed.
        /// </summary>
        /// <param name="scoresCallback">A callback which should be called when fetching is completed. Scheduling is not required.</param>
        /// <returns>An <see cref="APIRequest"/> responsible for the fetch operation. This will be queued and performed automatically.</returns>
        protected virtual APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback) => null;

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

        private void onExpandedPanelClicked()
        {
            statisticsPanel.ToggleVisibility();

            if (statisticsPanel.State.Value == Visibility.Hidden)
            {
                foreach (var panel in scorePanelList.Panels)
                {
                    if (panel.State == PanelState.Contracted)
                        panel.FadeIn(150);
                    else
                    {
                        panel.MoveTo(panel.GetTrackingPosition(), 150, Easing.OutQuint).OnComplete(p =>
                        {
                            scorePanelList.HandleScroll = true;
                            p.Tracking = true;
                        });
                    }
                }

                Background.FadeTo(0.5f, 150);
            }
            else
            {
                foreach (var panel in scorePanelList.Panels)
                {
                    if (panel.State == PanelState.Contracted)
                        panel.FadeOut(150, Easing.OutQuint);
                    else
                    {
                        scorePanelList.HandleScroll = false;

                        panel.Tracking = false;
                        panel.MoveTo(new Vector2(scorePanelList.CurrentScrollPosition + StatisticsPanel.SIDE_PADDING, panel.GetTrackingPosition().Y), 150, Easing.OutQuint);
                    }
                }

                Background.FadeTo(0.1f, 150);
            }
        }
    }
}
