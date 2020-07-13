using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Purcashe.Components;
using osuTK;
using osuTK.Graphics;
using static osu.Game.Screens.Purcashe.Components.ItemPanel;

namespace osu.Game.Screens.Purcashe.SubScreens
{
    public class RandomScreen : PurcasheBasicScreen
    {
        private FillFlowContainer panelFillFlow;
        private LoadingSpinner loadingSpinner;
        private OsuScrollContainer panelScroll;
        private TriangleButton unMaskAllButton;

        private Bindable<int> Coins = new Bindable<int>();
        private OsuSpriteText ppCountText;
        protected virtual string ScreenTitle => null;
        protected virtual int ItemCount => 0;

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            config.BindWith(MfSetting.EasterEggCoinCount, Coins);

            Content = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Height = 0.2f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.8f), Color4.Black.Opacity(0))
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = ScreenTitle ?? Title,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 60)
                                },
                                ppCountText = new OsuSpriteText
                                {
                                    Text = "剩余pp",
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Font = OsuFont.GetFont(size: 30)
                                },
                            }
                        }
                    }
                },
                panelScroll = new OsuScrollContainer(Direction.Horizontal)
                {
                    Depth = 1,
                    RelativeSizeAxes = Axes.Both,
                    Child = panelFillFlow = new FillFlowContainer
                    {
                        Name = "Panel Container",
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        LayoutDuration = 300,
                        LayoutEasing = Easing.OutQuint,
                        Padding = new MarginPadding{Top = (this.DrawHeight * 0.2f)},
                        Spacing = new Vector2(10)
                    },
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding{Bottom = 50},
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        unMaskAllButton = new TriangleButton
                        {
                            Size = new Vector2(120, 40),
                            Text = "打开所有",
                            Action = UnMaskAllPanels
                        },
                        new TriangleButton
                        {
                            Size = new Vector2(120, 40),
                            Text = "退出",
                            Action = () => this.Exit()
                        },
                        new TriangleButton
                        {
                            Size = new Vector2(120, 40),
                            Text = $"继续{ScreenTitle ?? "???"}",
                            Action = CreateAgain,
                        },
                    }
                },
                loadingSpinner = new LoadingSpinner(true, true),
            };

            panelScroll.ScrollContent.Anchor = Anchor.Centre;
            panelScroll.ScrollContent.Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Coins.BindValueChanged(v => ppCountText.Text = $"剩余pp: {v.NewValue}", true);
            loadingSpinner.Show();
            this.Delay(500).Schedule(() => CreatePanels());
        }

        //切换至后台
        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(300, Easing.OutQuint);

            return base.OnExiting(next);
        }

        //切换至前台
        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            this.FadeInFromZero(300, Easing.OutQuint);
        }

        private void CreatePanels(float delay = 100)
        {
            loadingSpinner.Hide();

            for (int i = 0; i < ItemCount; i++)
            {
                panelFillFlow.Add(new ItemPanel()
                {
                    Alpha = 0,
                    PPCount = RandomPP(),
                    Rank = RandomRank(),
                    TexturePath = RandomTexture(),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            var count = 0;
            foreach (var i in panelFillFlow)
            {
                i.Delay(delay * count).FadeIn(300);
                count++;
            }
        }

        private void CreateAgain()
        {
            CreatePanels(0);
        }

        private void UnMaskAllPanels()
        {
            foreach (var i in panelFillFlow)
            {
                 if (i is ItemPanel)
                     ((ItemPanel)i).UnMask();
            }
        }

        private int RandomPP()
        {
            return RNG.Next(-400, 400);
        }

        private RankStats RandomRank()
        {
            var rank = (RankStats)RNG.Next(0, (int)RankStats.Last);
            switch(rank)
            {
                case RankStats.Triangle:
                case RankStats.Beatmap:
                    if ( (ReturnRare)RNG.Next(0, (int)ReturnRare.last) != ReturnRare.yes)
                        rank = (RankStats)RNG.Next(0, (int)RankStats.Triangle);
                    break;

                default:
                    break;
            }

            return rank;
        }

        private string RandomTexture()
        {
            string[] textures=
            {
                "Backgrounds/bg1",
                "Backgrounds/bg2",
                "Backgrounds/bg3",
                //"Backgrounds/registration",
                "Menu/menu-background-1",
                "Menu/menu-background-2",
                "Menu/menu-background-3",
                "Menu/menu-background-4",
                "Menu/menu-background-5",
                "Menu/menu-background-6",
                "Menu/menu-background-7",
            };

            return textures[RNG.Next(0, textures.Length)];
        }
    }
}