// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osuTK;
using osuTK.Graphics;
using osu.Game.Users;

namespace osu.Game.Screens.Menu
{
    public class Disclaimer : StartupScreen
    {
        private SpriteIcon icon;
        private Color4 iconColour;
        private LinkFlowContainer textFlow;
        private LinkFlowContainer supportFlow;

        private Drawable heart;

        private const float icon_y = -85;
        private const float icon_size = 30;

        private readonly OsuScreen nextScreen;

        private readonly Bindable<User> currentUser = new Bindable<User>();
        private FillFlowContainer fill;

        private readonly List<Drawable> expendableText = new List<Drawable>();

        [CanBeNull]
        private Texture avatarTexture;

        private bool showDisclaimer;

        public Disclaimer(OsuScreen nextScreen = null, bool showDisclaimer = true)
        {
            this.nextScreen = nextScreen;
            this.showDisclaimer = showDisclaimer;
            ValidForResume = false;
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved(canBeNull: true)]
        private OsuGame game { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, TextureStore textures, CustomStore customStore, MConfigManager mConfig)
        {
            textures.AddStore(new TextureLoaderStore(customStore));
            var enableAvatarSprite = mConfig.Get<bool>(MSetting.UseCustomGreetingPicture);

            showDisclaimer = !mConfig.Get<bool>(MSetting.DoNotShowDisclaimer) && showDisclaimer;

            InternalChildren = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = OsuIcon.Logo,
                    Size = new Vector2(icon_size),
                    Y = icon_y,
                },
                fill = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Y = icon_y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        textFlow = new LinkFlowContainer
                        {
                            Width = 680,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(0, 2),
                        },
                    }
                },
                supportFlow = new LinkFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Padding = new MarginPadding(20),
                    Alpha = 0,
                    Spacing = new Vector2(0, 2),
                },
            };

            if (enableAvatarSprite)
            {
                avatarTexture = textures.Get("avatarlogo");

                AddInternal(new Sprite
                {
                    Size = new Vector2(400),
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = avatarTexture,
                    Colour = showDisclaimer ? Color4.Gray.Opacity(0.2f) : Color4.White,
                    Depth = float.MaxValue
                });
            }

            game.SetWindowIcon(mConfig.Get<string>(MSetting.CustomWindowIconPath));

            textFlow.AddText("这就是 osu!", t => t.Font = t.Font.With(Typeface.Torus, 30, FontWeight.Regular));

            expendableText.AddRange(textFlow.AddText("lazer", t =>
            {
                t.Font = t.Font.With(Typeface.Torus, 30, FontWeight.Regular);
                t.Colour = colours.PinkLight;
            }));

            static void formatRegular(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular);
            static void formatSemiBold(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold);

            textFlow.NewParagraph();

            textFlow.AddText("下一个osu!的", formatRegular);
            textFlow.AddText("重大更新", t =>
            {
                t.Font = t.Font.With(Typeface.Torus, 20, FontWeight.SemiBold);
                t.Colour = colours.Pink;
            });
            expendableText.AddRange(textFlow.AddText("即将到来！", formatRegular));
            textFlow.AddText(".", formatRegular);

            textFlow.NewParagraph();
            textFlow.NewParagraph();

            textFlow.AddParagraph("今日提示:", formatSemiBold);
            textFlow.AddParagraph(getRandomTip(), formatRegular);
            textFlow.NewParagraph();

            textFlow.NewParagraph();

            iconColour = colours.Yellow;

            // manually transfer the user once, but only do the final bind in LoadComplete to avoid thread woes (API scheduler could run while this screen is still loading).
            // the manual transfer is here to ensure all text content is loaded ahead of time as this is very early in the game load process and we want to avoid stutters.
            currentUser.Value = api.LocalUser.Value;
            currentUser.BindValueChanged(e =>
            {
                supportFlow.Children.ForEach(d => d.FadeOut().Expire());

                if (e.NewValue.IsSupporter)
                {
                    supportFlow.AddText("感谢支持osu!", formatSemiBold);
                }
                else
                {
                    supportFlow.AddText("您也可以考虑成为一名", formatSemiBold);
                    supportFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", formatSemiBold);
                    supportFlow.AddText("来支持游戏的开发", formatSemiBold);
                }

                heart = supportFlow.AddIcon(FontAwesome.Solid.Heart, t =>
                {
                    t.Padding = new MarginPadding { Left = 5, Top = 3 };
                    t.Font = t.Font.With(size: 20);
                    t.Origin = Anchor.Centre;
                    t.Colour = colours.Pink;
                }).First();

                if (IsLoaded)
                    animateHeart();

                if (supportFlow.IsPresent)
                    supportFlow.FadeInFromZero(500);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (nextScreen != null)
                LoadComponentAsync(nextScreen);

            ((IBindable<User>)currentUser).BindTo(api.LocalUser);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            game?.TransformWindowOpacity(1, 300);

            icon.RotateTo(10);
            icon.FadeOut();
            icon.ScaleTo(0.5f);

            icon.Delay(500).FadeIn(500).ScaleTo(1, 500, Easing.OutQuint);

            if (showDisclaimer)
            {
                using (BeginDelayedSequence(3000, true))
                {
                    icon.FadeColour(iconColour, 200, Easing.OutQuint);
                    icon.MoveToY(icon_y * 1.3f, 500, Easing.OutCirc)
                        .RotateTo(-360, 520, Easing.OutQuint)
                        .Then()
                        .MoveToY(icon_y, 160, Easing.InQuart)
                        .FadeColour(Color4.White, 160);

                    using (BeginDelayedSequence(520 + 160))
                    {
                        fill.MoveToOffset(new Vector2(0, 15), 160, Easing.OutQuart);
                        Schedule(() => expendableText.ForEach(t =>
                        {
                            t.FadeOut(100);
                            t.ScaleTo(new Vector2(0, 1), 100, Easing.OutQuart);
                        }));
                    }
                }

                supportFlow.FadeOut().Delay(2000).FadeIn(500);
                double delay = 500;
                foreach (var c in textFlow.Children)
                    c.FadeTo(0.001f).Delay(delay += 20).FadeIn(500);

                animateHeart();

                this
                    .FadeInFromZero(500)
                    .Then(5500)
                    .FadeOut(250)
                    .ScaleTo(0.9f, 250, Easing.InQuint)
                    .Finally(d =>
                    {
                        if (nextScreen != null)
                            this.Push(nextScreen);
                    });
            }
            else
            {
                var instantPush = avatarTexture == null;

                icon.FadeOut();
                textFlow.FadeOut();
                supportFlow.FadeOut();

                if (!instantPush)
                {
                    supportFlow.Delay(2000).FadeIn(500);

                    animateHeart();
                }

                this
                    .FadeInFromZero(instantPush ? 0 : 500)
                    .Then(instantPush ? 0 : 3500)
                    .FadeOut(250)
                    .ScaleTo(0.9f, 250, Easing.InQuint)
                    .Finally(d =>
                    {
                        if (nextScreen != null)
                            this.Push(nextScreen);
                    });
            }
        }

        private string getRandomTip()
        {
            string[] tips =
            {
                "您可以在游戏中的任何位置按Ctrl+T来切换顶栏!",
                "您可以在游戏中的任何位置按Ctrl+O来访问设置!",
                "所有设置都是动态的，并实时生效。试试在游戏时时更改皮肤!",
                "每一次更新都会携带全新的功能。确保您的游戏为最新版本!",
                "如果您发现UI太大或太小，那么试试更改设置中的界面缩放!",
                "试着调整“屏幕缩放”模式，即使在全屏模式下也可以更改游戏或UI区域！",
                "目前，osu!direct对所有使用lazer的用户可用。您可以使用Ctrl+D在任何地方访问它！",
                "看到回放界面下面的时间条没？拖动他试试！",
                "多线程模式允许您即使在低帧数的情况下也能拥有准确的判定！",
                "在mod选择面板中向下滚动可以找到一堆有趣的新mod！",
                "大部分web内容(玩家资料,在线排名等)在游戏内已有原生支持！点点看顶栏上的图标！",
                "右键一个谱面可以选择查看在线信息，隐藏该谱面甚至删除单个难度！",
                "所有删除操作在退出游戏前都是临时的！您可以在“维护”设置中选择恢复被意外删除的内容！",
                "看看多人游戏中的“时移”玩法，他具备房间排行榜和游玩列表的功能！",
                "您可以在游戏中按Ctrl+F11来切换高级fps显示功能！",
                "使用Ctrl+F2来查看详细性能记录！",
                "看看\"游玩列表\"系统, 他允许用户创建自己的自定义排行榜和永久排行榜!",
                "owo"
            };

            return tips[RNG.Next(0, tips.Length)];
        }

        private void animateHeart()
        {
            heart.FlashColour(Color4.White, 750, Easing.OutQuint).Loop();
        }
    }
}
