using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Purcashe.SubScreens;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Purcashe
{
    public class PurcasheDashBoardScreen : PurcasheBasicScreen
    {
        private Container contentContainer;
        private FillFlowContainer buttonsFillFlow;

        private readonly Bindable<int> pp = new Bindable<int>();
        private OsuSpriteText ppCountText;
        private OsuSpriteText versionTitle;
        private OsuSpriteText mainTitle;
        private OsuTextBox textBox;
        private ShakeContainer shake;
        private MConfigManager config;

        [Resolved(CanBeNull = true)]
        private NotificationOverlay notifications { get; set; }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, MConfigManager config)
        {
            this.config = config;
            config.BindWith(MSetting.PPCount, pp);

            Content = new Drawable[]
            {
                new FillFlowContainer
                {
                    Name = "Base Container",
                    RelativeSizeAxes = Axes.Both,
                    LayoutEasing = Easing.OutQuint,
                    LayoutDuration = ANIM_DURATION,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f),
                    Spacing = new Vector2(20),
                    Children = new Drawable[]
                    {
                        contentContainer = new Container
                        {
                            Name = "Content Container",
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Masking = true,
                            CornerRadius = 25f,
                            Children = new Drawable[]
                            {
                                new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    FillMode = FillMode.Fill,
                                    Texture = textures.Get("Backgrounds/registration")
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(0, 0, 0, 0.7f),
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
                                        versionTitle = new OsuSpriteText
                                        {
                                            Font = OsuFont.Numeric,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        },
                                        mainTitle = new OsuSpriteText
                                        {
                                            Font = OsuFont.GetFont(size: 60, weight: FontWeight.Black),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Colour = Color4.Gold
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
                                    Margin = new MarginPadding { Top = 50 },
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
                                            Colour = new Color4(0, 0, 0, 0.5f)
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
                                            LayoutDuration = ANIM_DURATION,
                                            LayoutEasing = Easing.OutQuint,
                                            Name = "Button FillFlow",
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.9f,
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            Margin = new MarginPadding { Vertical = 15 },
                                            Spacing = new Vector2(10),
                                            Children = new Drawable[]
                                            {
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "单抽",
                                                    Action = () => calculateRemaingPP(1)
                                                },
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "十连",
                                                    Action = () => calculateRemaingPP(10)
                                                },
                                                new TriangleButton
                                                {
                                                    Alpha = 0,
                                                    Width = 120,
                                                    Text = "重置",
                                                    Action = () =>
                                                    {
                                                        config.Set(MSetting.PurcasheBgBeatmap, false);
                                                        config.Set(MSetting.PurcasheBgTriangles, false);
                                                        config.Set(MSetting.PPCount, 500);
                                                    }
                                                },
                                                shake = new ShakeContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Height = 50,
                                                    Child = textBox = new OsuTextBox
                                                    {
                                                        Height = 50,
                                                        RelativeSizeAxes = Axes.X,
                                                        PlaceholderText = "在这里输入你想随机的次数"
                                                    }
                                                }
                                            }
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
                                            Origin = Anchor.BottomCentre
                                        },
                                    }
                                }
                            }
                        }
                    }
                }
            };

            rollMeme();
            textBox.OnCommit += TextBoxOnOnCommit;
        }

        private void TextBoxOnOnCommit(TextBox sender, bool newtext)
        {
            try
            {
                var times = int.Parse(sender.Text);
                if (!calculateRemaingPP(times, true))
                    shake.Shake();
            }
            catch (Exception e)
            {
                Logger.Log(e.ToString());
                shake.Shake();
            }
        }

        private bool calculateRemaingPP(int times, bool isCustom = false)
        {
            var current = config.Get<int>(MSetting.PPCount);

            if (times < 0)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "随机次数不能小于0!",
                    Icon = FontAwesome.Solid.Exclamation
                });

                return false;
            }

            if (current - times * 50 < 0)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "你没有足够的PP!",
                    Icon = FontAwesome.Solid.Exclamation
                });

                return false;
            }

            this.Push(new CustomRandomScreen
            {
                RandomTimes = times,
                IsCustom = isCustom
            });

            return true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ResizeTo(0.8f).Then().ResizeTo(1, ANIM_DURATION, Easing.OutQuint);

            contentContainer.FadeIn(ANIM_DURATION, Easing.OutQuint);

            const int time = 300;
            var count = 0;

            foreach (var i in buttonsFillFlow)
            {
                i.Anchor = Anchor.Centre;
                i.Origin = Anchor.Centre;

                i.Delay(time + time * count).FadeIn(ANIM_DURATION, Easing.OutQuint);
                count++;
            }

            pp.BindValueChanged(v => ppCountText.Text = $"剩余pp: {v.NewValue}, 抽取一次将花费50pp", true);
        }

        private void rollMeme()
        {
            int subVersion = RNG.Next(0, 3);

            versionTitle.Text = $"osu! 1.{subVersion} ver.";

            switch (subVersion)
            {
                case 0:
                    mainTitle.Text = "未归的白猫";
                    break;

                case 1:
                    mainTitle.Text = "迫近的peppy";
                    break;

                case 2:
                    mainTitle.Text = "Sotarks与Cookiezi";
                    break;
            }
        }
    }
}
