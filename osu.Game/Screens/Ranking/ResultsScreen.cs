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
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public abstract class ResultsScreen : OsuScreen
    {
        protected const float BACKGROUND_BLUR = 20;

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

        private Drawable bottomPanel;
        private ScorePanelList panels;

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
                        new ResultsScrollContainer
                        {
                            Child = panels = new ScorePanelList
                            {
                                RelativeSizeAxes = Axes.Both,
                                SelectedScore = { BindTarget = SelectedScore }
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
                panels.AddScore(Score);

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
                    panels.AddScore(s);
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

        private class ResultsScrollContainer : OsuScrollContainer
        {
            private readonly Container content;

            protected override Container<Drawable> Content => content;

            public ResultsScrollContainer()
            {
                base.Content.Add(content = new Container
                {
                    RelativeSizeAxes = Axes.X
                });

                RelativeSizeAxes = Axes.Both;
                ScrollbarVisible = false;
            }

            protected override void Update()
            {
                base.Update();
                content.Height = Math.Max(768 - TwoLayerButton.SIZE_EXTENDED.Y, DrawHeight);
            }
        }
    }
}
