using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Purcashe.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using static osu.Game.Screens.Purcashe.Components.ItemPanel;

namespace osu.Game.Screens.Purcashe.SubScreens
{
    public class RandomScreen : PurcasheBasicScreen
    {
        private FillFlowContainer<ItemPanel> panelFillFlow;
        private OsuScrollContainer panelScroll;

        private readonly Bindable<int> coins = new Bindable<int>();
        private OsuSpriteText ppCountText;
        protected virtual string ScreenTitle => null;
        protected virtual int ItemCount => 0;

        public override bool AllowExternalScreenChange => false;
        public override bool AllowBackButton => allowBack;
        public override float BackgroundParallaxAmount => 3f;

        private readonly BindableInt tempPPCount = new BindableInt();
        private Container overlay;
        private MystryBox box;
        private bool allowBack;

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            config.BindWith(MSetting.PPCount, coins);
            tempPPCount.Value = coins.Value;
            Scale = new Vector2(0.9f);

            Content = new Drawable[]
            {
                panelScroll = new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Scale = new Vector2(1.5f),
                    ScrollContent = { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                    ScrollbarVisible = false,
                    Child = panelFillFlow = new FillFlowContainer<ItemPanel>
                    {
                        Name = "Panel Container",
                        AutoSizeAxes = Axes.X,
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 150 },
                        Padding = new MarginPadding { Top = DrawHeight * 0.2f },
                        Spacing = new Vector2(10)
                    },
                },
                box = new MystryBox(ItemCount > 1)
                {
                    OnFireAction = () => this.Push(new RandomShowcaseScreen
                    {
                        Results = results
                    })
                },
                overlay = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
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
                                        }
                                    }
                                }
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding { Bottom = 50 },
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                new TriangleButton
                                {
                                    Size = new Vector2(120, 40),
                                    Text = "退出",
                                    Action = this.Exit
                                }
                            }
                        },
                    }
                },
            };
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            unMask();
        }

        private void unMask()
        {
            foreach (var p in panelFillFlow)
            {
                p.ApplyResult();
            }

            this.Delay(300).Schedule(() =>
            {
                allowBack = true;
                panelScroll.FadeIn(300).ScaleTo(1, 300, Easing.OutQuint);
                overlay.Delay(600).FadeIn(300);
            });

            coins.Value = tempPPCount.Value;
            if (coins.Value < 0) coins.Value = 0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(1, 300, Easing.OutQuint);
            tempPPCount.BindValueChanged(v => ppCountText.Text = $"剩余pp: {v.NewValue}", true);
            createPanels();
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            ApplyToBackground(b =>
            {
                b.FadeTo(0.5f, 250);
            });
        }

        public override bool OnExiting(IScreen next)
        {
            this.FadeOut(300, Easing.OutQuint);

            if (!this.IsCurrentScreen()) return base.OnExiting(next);

            ApplyToBackground(b =>
            {
                b.FadeTo(1, 250);
            });

            return base.OnExiting(next);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    box.Click();
                    break;
            }

            return base.OnKeyDown(e);
        }

        private readonly List<RollResult> results = new List<RollResult>();

        private void createPanels(float delay = 100)
        {
            tempPPCount.Value -= ItemCount * 50;

            if (tempPPCount.Value < 0)
            {
                tempPPCount.Value = coins.Value;

                notifications?.Post(new SimpleNotification
                {
                    Text = "你没有足够的PP!",
                    Icon = FontAwesome.Solid.Exclamation
                });

                this.Delay(300).Schedule(this.Exit);
                return;
            }

            for (int i = 0; i < ItemCount; i++)
            {
                var pp = randomPP();
                var rank = Rank.Oops;
                var lv = randomLevel();
                var texturePath = randomTexture();
                string name = pp + "pp";

                //其他设定
                switch (lv)
                {
                    case LevelStats.Triangle:
                        name = "粒子动画";
                        pp = 1000;
                        break;

                    case LevelStats.Beatmap:
                        name = "谱面背景";
                        pp = 1000;
                        break;

                    case LevelStats.Pippi:
                        name = "Pippi";
                        texturePath = "Backgrounds/registration";
                        pp = 500;
                        break;
                }

                if (pp >= 0 && pp < 300) rank = Rank.Common;
                if (pp >= 300 && pp < 501) rank = Rank.Rare;
                if (pp >= 501) rank = Rank.Legendary;

                RollResult result = new RollResult
                {
                    Level = lv,
                    PP = pp,
                    Rank = rank,
                    RollName = name,
                    TexturePath = texturePath
                };

                results.Add(result);

                panelFillFlow.Add(new ItemPanel
                {
                    RollResult = result,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    PP = { BindTarget = tempPPCount }
                });
            }

            Rank bst = Rank.Oops;

            foreach (var i in panelFillFlow)
            {
                if (i.RollResult.Rank > bst)
                    bst = i.RollResult.Rank;
            }

            switch (bst)
            {
                case Rank.Oops:
                    box.GlowColor = Color4.DarkRed;
                    break;

                case Rank.Common:
                    box.GlowColor = Color4.LightBlue;
                    break;

                case Rank.Rare:
                    box.GlowColor = Color4.Silver;
                    break;

                case Rank.Legendary:
                    box.GlowColor = Color4.Gold;
                    break;
            }
        }

        private int randomPP() => RNG.Next(-400, 400);

        private LevelStats randomLevel()
        {
            var rank = (LevelStats)RNG.Next(0, (int)LevelStats.Last);

            switch (rank)
            {
                case LevelStats.Triangle:
                case LevelStats.Beatmap:
                    if ((ReturnRare)RNG.Next(0, (int)ReturnRare.last) != ReturnRare.yes)
                        rank = (LevelStats)RNG.Next(0, (int)LevelStats.Triangle);
                    break;
            }

            return rank;
        }

        private string randomTexture()
        {
            string[] textures =
            {
                "Backgrounds/bg1",
                "Backgrounds/bg2",
                "Backgrounds/bg3",
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
