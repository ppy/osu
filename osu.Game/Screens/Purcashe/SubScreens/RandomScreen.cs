using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
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
using osu.Game.Screens.Purcashe.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using static osu.Game.Screens.Purcashe.Components.ItemPanel;

namespace osu.Game.Screens.Purcashe.SubScreens
{
    public abstract class RandomScreen : PurcasheBasicScreen
    {
        private FillFlowContainer<ItemPanel> panelFillFlow;
        private OsuScrollContainer panelScroll;

        private readonly Bindable<int> coins = new Bindable<int>();
        private OsuSpriteText ppCountText;
        public virtual string ScreenTitle => null;
        public virtual int ItemCount => 0;
        public bool IsCustom;

        public override bool AllowExternalScreenChange => false;
        public override bool AllowBackButton => allowBack;
        public override float BackgroundParallaxAmount => 3f;

        private readonly BindableInt tempPPCount = new BindableInt();
        private Container overlay;
        private MystryBox box;
        private bool allowBack;
        private DrawableSample loop;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, AudioManager audio)
        {
            loop = new DrawableSample(audio.Samples.Get("Gameplay/pause-loop"));

            loop.GetChannel().Looping = true;

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
                box = new MystryBox(ItemCount, loop)
                {
                    OnFireAction = () => this.Push(new RandomShowcaseScreen
                    {
                        Results = results,
                        IsCustom = IsCustom
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
                        }
                    }
                },
            };
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (results.Count == 0)
            {
                panelScroll.Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Alpha = 0.8f,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.HorseHead,
                                    Size = new Vector2(80),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                },
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.Question,
                                    Size = new Vector2(80),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Text = "没有结果",
                            Font = OsuFont.GetFont(size: 80),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                });
            }

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

            commitPP();
        }

        private void commitPP()
        {
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
            loop.VolumeTo(0, 300, Easing.Out).Expire();

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

        private void createPanels()
        {
            tempPPCount.Value -= ItemCount * 50;

            for (int crt = 0; crt < ItemCount; crt++)
            {
                var pp = randomPP();
                var rank = Rank.Oops;
                var lv = randomLevel();
                var texturePath = randomTexture();

                //其他设定

                //十连保底一个Rare
                //随机次数=9(10-1) 并且 results中没有Rank>=Rank.Rare的结果
                if ((1 + crt) % 10 == 0 && crt > 0)
                {
                    //crt - 9加上crt为本次十连, 取9个
                    var last = results.GetRange(crt - 9, 9);

                    if (!last.Any(s => s.Rank >= Rank.Rare))
                        //[300, 501)
                        pp = RNG.Next(300, 501);
                }

                //百连保底必出一金
                //随机次数=99(100-1) 并且 results中没有Level>Regular的结果
                if ((1 + crt) % 100 == 0 && crt > 0)
                {
                    //crt - 99加上crt为本次十连, 取99个
                    var last = results.GetRange(crt - 99, 99);

                    if (!last.Any(s => s.Level > LevelStats.Regular))
                        lv = randomLevel(true);
                }

                string name = pp + "pp";

                //根据LevelStats决定是否覆盖当前pp和name
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
                        pp = 600;
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
                case Rank.Legendary:
                    loop.FrequencyTo(1.5f);
                    break;

                case Rank.Rare:
                    loop.FrequencyTo(1);
                    break;

                case Rank.Common:
                    loop.FrequencyTo(0.75f);
                    break;

                case Rank.Oops:
                    loop.FrequencyTo(0.5f);
                    break;
            }

            loop.VolumeTo(0.2f);
            loop.Play();

            box.GlowColor = PurcasheColorProvider.GetColor(bst);

            //在这里提交一次数值
            commitPP();
        }

        private int randomPP() => RNG.Next(-400, 400);

        private LevelStats randomLevel(bool force = false)
        {
            LevelStats level;

            //[0, 50) -> 1/50 -> 2%
            if (RNG.Next(0, 51) == 1 || force)
                level = (LevelStats)RNG.Next((int)LevelStats.Triangle, (int)LevelStats.Last);
            else
                level = LevelStats.Regular;

            return level;
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
