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
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
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

        private Bindable<bool> OptUIEnabled;
        FillFlowContainer buttons;
        OsuSpriteText texts;
        Box colorBox;
        private const float DURATION = 500;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 50);

        private BottomPanel bottomPanel;
        private ScorePanelList panels;

        protected ResultsScreen(ScoreInfo score, bool allowRetry = true)
        {
            Score = score;
            this.allowRetry = allowRetry;

            SelectedScore.Value = score;
        }

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            OptUIEnabled = config.GetBindable<bool>(MfSetting.OptUI);

            InternalChildren = new Drawable[]
            {
                new ParallaxContainer
                {
                    Masking = true,
                    Child = new MfBgTriangles(0.5f, false, 5f),
                },
                new GridContainer
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
                            bottomPanel = new BottomPanel
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = TwoLayerButton.SIZE_EXTENDED.Y,
                                Alpha = 0,
                                Children = new Drawable[]
                                {
                                    colorBox = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4Extensions.FromHex("#333")
                                    },
                                    new Container
                                    {
                                        Name = "Base Container",
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        AutoSizeAxes = Axes.X,
                                        RelativeSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            texts = new OsuSpriteText
                                            {
                                                Name = "Texts Fillflow",
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Margin = new MarginPadding{ Top = 10 },
                                                Y = -10,
                                            },
                                            buttons = new FillFlowContainer
                                            {
                                                Anchor = Anchor.BottomCentre,
                                                Origin = Anchor.BottomCentre,
                                                AutoSizeAxes = Axes.Both,
                                                Margin = new MarginPadding{ Bottom = 7.5f },
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
                },
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

            bottomPanel.panel_IsHovered.BindValueChanged( _ => UpdateVisualEffects());
            UpdateRankTexts();

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

            texts.Hide();
            switch (OptUIEnabled.Value)
            {
                case true:
                    bottomPanel.Y = TwoLayerButton.SIZE_EXTENDED.Y;
                    bottomPanel.Delay(250).FadeTo(1, 200).MoveToY(0, 550, Easing.OutBack);

                    buttons.FadeTo(0).MoveToX(200)
                           .Then().Delay(250)
                           .Then().MoveToX(0, 550, Easing.OutQuint).FadeIn(200);

                    panels.MoveToY(DrawHeight)
                                      .Then().Delay(250)
                                      .Then().MoveToY(0, 750, Easing.OutExpo);
                    break;

                case false:
                    bottomPanel.FadeTo(1, 250);
                    break;
            };
        }

        public override bool OnExiting(IScreen next)
        {
            Background.FadeTo(1, 250);

            switch (OptUIEnabled.Value)
            {
                case true:
                    bottomPanel.MoveToY(TwoLayerButton.SIZE_EXTENDED.Y, 250);
                    this.FadeOut(100);
                    break;
            };

            return base.OnExiting(next);
        }
        private void UpdateRankTexts()
        {
                switch ( Score.Rank )
                {
                    case ScoreRank.X:
                    case ScoreRank.XH:
                        texts.Text = RandomTextXH();
                        break;

                    case ScoreRank.S:
                    case ScoreRank.SH:
                        texts.Text = RandomTextS();
                        break;

                    case ScoreRank.A:
                        texts.Text = "快要到了, 继续努力!";
                        break;

                    case ScoreRank.B:
                    case ScoreRank.C:
                    case ScoreRank.D:
                        texts.Text = "加油, 你一定能行!";
                        break;

                    default:
                        texts.Text = "???";
                        break;
                }
        }
        private string RandomTextXH() //基于Disclaimer
        {
            string[] texts =
            {
                "恭喜达成SS!",
                "去挑战top榜吧!",
                "OHHHHHHHHHHHHH",
            };

            return texts[RNG.Next(0, texts.Length)];
        }

        private void UpdateVisualEffects()
        {
            if (OptUIEnabled.Value)
            switch(bottomPanel.panel_IsHovered.Value)
            {
                case true:
                    bottomPanel.ResizeHeightTo(BOTTOMPANEL_SIZE.Y + 30, DURATION, Easing.OutQuint);
                    colorBox.FadeColour( Color4Extensions.FromHex("#2d2d2d"), DURATION);
                    texts.FadeIn(DURATION).MoveToY(-23, DURATION, Easing.OutQuint);
                    break;

                case false:
                    bottomPanel.ResizeHeightTo(BOTTOMPANEL_SIZE.Y, DURATION, Easing.OutQuint);
                    colorBox.FadeColour( Color4Extensions.FromHex("#333"), DURATION );
                    texts.FadeOut(DURATION, Easing.OutExpo).MoveToY(-10, DURATION, Easing.OutQuint);
                    break;
            }
        }

        private string RandomTextS()
        {
            string[] texts =
            {
                "虽然有点难, 但你克服了几乎所有的挑战!",
                "离SS只有咫尺之遥!",
            };

            return texts[RNG.Next(0, texts.Length)];
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
