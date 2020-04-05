// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Scoring;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public class ResultsScreen : OsuScreen
    {
        private Bindable<bool> OptUIEnabled;
        private static readonly Vector2 BOTTOMPANEL_SIZE = new Vector2(TwoLayerButton.SIZE_EXTENDED.X, 60);
        protected const float BACKGROUND_BLUR = 20;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        // Temporary for now to stop dual transitions. Should respect the current toolbar mode, but there's no way to do so currently.
        public override bool HideOverlaysOnEnter => true;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        [Resolved(CanBeNull = true)]
        private Player player { get; set; }

        public readonly ScoreInfo Score;
        FillFlowContainer buttons;
        OsuSpriteText texts;
        Box colorBox;
        private readonly bool allowRetry;
        private Drawable drawableBottomPanel;
        private BottomPanel bottomPanel;
        private const float DURATION = 500;

        private Graphics.Mf.Resources.ParallaxContainer scorePanelParallax;

        public ResultsScreen(ScoreInfo score, bool allowRetry = true)
        {
            Score = score;
            this.allowRetry = allowRetry;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            OptUIEnabled = config.GetBindable<bool>(OsuSetting.OptUI);

            InternalChildren = new[]
            {
                new ParallaxContainer
                {
                    Masking = true,
                    Child = new MfBgTriangles(0.5f, false, 5f),
                },
                scorePanelParallax = new Graphics.Mf.Resources.ParallaxContainer
                {
                    Masking = true,
                    ParallaxAmount = 0.01f,
                    Child = new ResultsScrollContainer
                    {
                        Child = new ScorePanel(Score)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            State = PanelState.Expanded
                        },
                    },
                },
                drawableBottomPanel = bottomPanel = new BottomPanel
                {
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
                                    Y = -10,
                                },
                                buttons = new FillFlowContainer
                                {
                                    Name = "Buttons FillFlow",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.X,
                                    Height = BOTTOMPANEL_SIZE.Y - 10,
                                    Y = 5,
                                    Spacing = new Vector2(5),
                                    Direction = FillDirection.Horizontal,
                                    Children = new Drawable[]
                                    {
                                        new ReplayDownloadButton(Score) { Width = 300 },
                                    }
                                }
                            }
                        },

                    }
                }
            };

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
            bottomPanel.panel_IsHovered.ValueChanged += _ => UpdateVisualEffects();
            base.LoadComplete();
        }

        private void UpdateVisualEffects()
        {
            if (OptUIEnabled.Value)
            switch(bottomPanel.panel_IsHovered.Value)
            {
                case true:
                    bottomPanel.ResizeHeightTo(BOTTOMPANEL_SIZE.Y + 30, DURATION, Easing.OutQuint);
                    buttons.MoveToY(20, DURATION, Easing.OutQuint);
                    colorBox.FadeColour( Color4Extensions.FromHex("#2d2d2d"), DURATION);
                    switch ( Score.Rank )
                    {
                        case ScoreRank.X:
                        case ScoreRank.XH:
                            texts.Text = "加入俱乐部吧, 我们需要你这样的人!";
                            break;

                        case ScoreRank.S:
                        case ScoreRank.SH:
                            texts.Text = "虽然有点难, 但你克服了几乎所有的挑战!";
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
                    texts.FadeIn(DURATION).MoveToY(-23, DURATION, Easing.OutQuint);
                    break;

                case false:
                    bottomPanel.ResizeHeightTo(BOTTOMPANEL_SIZE.Y, DURATION, Easing.OutQuint);
                    buttons.MoveToY(5, DURATION, Easing.OutQuint);
                    colorBox.FadeColour( Color4Extensions.FromHex("#333"), DURATION );
                    texts.FadeOut(DURATION, Easing.OutExpo).MoveToY(-10, DURATION, Easing.OutQuint);
                    break;
            }
        }

        private string getRandomText() //之后会用上, 取自Disclaimer
        {
            string[] texts =
            {
                "句子1",
                "句子2",
            };

            return texts[RNG.Next(0, texts.Length)];
        }
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
                    bottomPanel.Delay(250).FadeTo(1, 200).MoveToY(10, 550, Easing.OutBack);

                    buttons.FadeTo(0).MoveToX(200)
                           .Then().Delay(250)
                           .Then().MoveToX(0, 550, Easing.OutQuint).FadeIn(200);

                    scorePanelParallax.MoveToY(DrawHeight)
                                      .Then().Delay(250)
                                      .Then().MoveToY(0, 750, Easing.OutExpo);
                    break;

                case false:
                    bottomPanel.MoveToY(10).FadeTo(1, 250);
                    break;
            };
        }

        public override bool OnExiting(IScreen next)
        {
            Background.FadeTo(1, 250);
            switch (OptUIEnabled.Value)
            {
                case true:
                    bottomPanel.FadeTo(0, 250).MoveToY(TwoLayerButton.SIZE_EXTENDED.Y, 250);
                    this.FadeOut(100);
                    break;
            };
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
                content.Height = Math.Max(768, DrawHeight);
            }
        }
    }
}
