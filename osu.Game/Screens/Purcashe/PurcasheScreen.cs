using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Purcashe;
using osu.Game.Screens.Purcashe.SubScreens;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens
{
    public class PurcasheScreen : PurcasheBasicScreen
    {
        private Container contentContainer;
        private FillFlowContainer buttonsFillFlow;
        private Container avatarScroll;
        private FillFlowContainer baseContainer;

        private Bindable<int> Coins = new Bindable<int>();
        private OsuSpriteText ppCountText;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, MfConfigManager config)
        {
            config.BindWith(MfSetting.EasterEggCoinCount, Coins);

            Content = new Drawable[]
            {
                baseContainer = new FillFlowContainer
                {
                    Name = "Base Container",
                    RelativeSizeAxes = Axes.Both,
                    LayoutEasing = Easing.OutQuint,
                    LayoutDuration = anim_duration,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new osuTK.Vector2(0.8f),
                    Spacing = new Vector2(20),
                    Children = new Drawable[]
                    {
                        avatarScroll = new Container
                        {
                            Alpha = 0,
                            Name = "Avatar Container",
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 0.35f,
                            Masking = true,
                            CornerRadius = 25f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(0,0,0,0.7f),
                                },
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    FillMode = FillMode.Fill,
                                    Texture = textures.Get("Backgrounds/registration"),
                                }
                            }
                        },
                        contentContainer = new Container
                        {
                            Name = "Content Container",
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Width = 0.55f,
                            Masking = true,
                            CornerRadius = 25f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(0,0,0,0.7f),
                                },
                                new FillFlowContainer
                                {
                                    Name = "Title FillFlow",
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Height = 0.5f,
                                    Spacing = new Vector2(10),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = "限时特惠，立省50%",
                                            Font = OsuFont.GetFont(size: 60),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "早买早享受，晚买享折扣，不买免费送！",
                                            Font = OsuFont.GetFont(size: 30),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "最高可拿1000pp！",
                                            Font = OsuFont.GetFont(size: 30),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        }
                                    }
                                },
                                new Container
                                {
                                    CornerRadius = 12.5f,
                                    Masking = true,
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(0.6f, 0.1f),
                                    Margin = new MarginPadding{Top = 50},
                                    Children = new Drawable[]
                                    {
                                        new Sprite
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            FillMode = FillMode.Fill,
                                            RelativeSizeAxes = Axes.Both,
                                            Texture = textures.Get("Backgrounds/bg4"),
                                        },
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = new Color4(0,0,0,0.5f)
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "本期up: Pippi",
                                            Font = OsuFont.GetFont(size: 24)
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 0.4f,
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    Children = new Drawable[]
                                    {
                                        buttonsFillFlow = new FillFlowContainer
                                        {
                                            LayoutDuration = anim_duration,
                                            LayoutEasing = Easing.OutQuint,
                                            Name = "Button FillFlow",
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.9f,
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            Margin = new MarginPadding{Vertical = 15},
                                            Spacing = new Vector2(10),
                                            Children = new Drawable[]
                                            {
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "单抽",
                                                    Action = () => this.Push(new RandomOnceScreen())
                                                },
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "十连",
                                                    Action = () => this.Push(new RandomTenTimesScreen())
                                                },
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "重置",
                                                    Action = () =>
                                                    {
                                                        config.Set(MfSetting.EasterEggBGTriangle, false);
                                                        config.Set(MfSetting.EasterEggBGBeatmap, false);
                                                        config.Set(MfSetting.EasterEggCoinCount, 1000);
                                                    }
                                                },
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "退出",
                                                    Action = () => this.Exit()
                                                },
                                            }
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "打发打发时间就行了, 请不要沉迷其中 ;)",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "不会有任何真实货币将被消耗, 也不会产生任何流量, 请放心游玩",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                        },
                                        ppCountText = new OsuSpriteText
                                        {
                                            Text = "剩余pp",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ResizeTo(0.8f).Then().ResizeTo(1, anim_duration, Easing.OutQuint);

            contentContainer.FadeIn(anim_duration, Easing.OutQuint);

            avatarScroll.Delay(anim_duration).FadeTo(0.01f).Then().Delay(anim_duration).FadeIn(anim_duration, Easing.OutQuint);

            var time = 300;
            var count = 0;
            foreach(var i in buttonsFillFlow)
            {
                i.Anchor = Anchor.Centre;
                i.Origin = Anchor.Centre;

                i.Delay(time + time*count).FadeIn(anim_duration, Easing.OutQuint);
                count++;
            }

            Coins.BindValueChanged(v => ppCountText.Text = $"剩余pp: {v.NewValue}, 抽取一次将花费50pp", true);
        }
    }
}