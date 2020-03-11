// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Utils;
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

        public Disclaimer(OsuScreen nextScreen = null)
        {
            this.nextScreen = nextScreen;
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IAPIProvider api)
        {
            InternalChildren = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.Poo,
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
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Spacing = new Vector2(0, 2),
                            LayoutDuration = 2000,
                            LayoutEasing = Easing.OutQuint
                        },
                        supportFlow = new LinkFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Alpha = 0,
                            Spacing = new Vector2(0, 2),
                        },
                    }
                }
            };

            textFlow.NewParagraph();
            textFlow.NewParagraph();
            textFlow.AddText("注意, 这是一个", t => t.Font = t.Font.With(Typeface.Exo, 30, FontWeight.Light));
            textFlow.AddText("分支版本", t => t.Font = t.Font.With(Typeface.Exo, 30, FontWeight.SemiBold));

            textFlow.AddParagraph("一些功能可能不会像预期或最新版的那样工作", t => t.Font = t.Font.With(size: 25));
            textFlow.NewParagraph();

            static void format(SpriteText t) => t.Font = OsuFont.GetFont(size: 15, weight: FontWeight.SemiBold);

            textFlow.AddParagraph(getRandomTip(), t => t.Font = t.Font.With(Typeface.Exo, 20, FontWeight.SemiBold));
            textFlow.NewParagraph();

            textFlow.NewParagraph();

            iconColour = colours.Yellow;

            currentUser.BindTo(api.LocalUser);
            currentUser.BindValueChanged(e =>
            {
                supportFlow.Children.ForEach(d => d.FadeOut().Expire());

                if (e.NewValue.IsSupporter)
                {
                    supportFlow.AddText("永远感谢您支持osu!", format);
                }
                else
                {
                    supportFlow.AddText("您也可以考虑成为一名", format);
                    supportFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", creationParameters: format);
                    supportFlow.AddText("来支持游戏的开发", format);
                }

                heart = supportFlow.AddIcon(FontAwesome.Solid.Heart, t =>
                {
                    t.Padding = new MarginPadding { Left = 5, Top = 3 };
                    t.Font = t.Font.With(size: 12);
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
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            icon.RotateTo(10);
            icon.FadeOut();
            icon.ScaleTo(0.5f);

            icon.Delay(500).FadeIn(500).ScaleTo(1, 500, Easing.OutQuint);

            using (BeginDelayedSequence(3000, true))
            {
                icon.FadeColour(iconColour, 200, Easing.OutQuint);
                icon.MoveToY(icon_y * 1.3f, 500, Easing.OutCirc)
                    .RotateTo(-360, 520, Easing.OutQuint)
                    .Then()
                    .MoveToY(icon_y, 160, Easing.InQuart)
                    .FadeColour(Color4.White, 160);

                fill.Delay(520 + 160).MoveToOffset(new Vector2(0, 15), 160, Easing.OutQuart);
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
                "目前，osu!direct对所有使用lazer上的用户可用。您可以使用Ctrl+D在任何地方访问它！",
                "看到回放界面下面的时间条没？拖动他试试！",
                "多线程模式允许您即使在低帧数的情况下也能拥有准确的判定！",
                "在mod选择面板中向下滚动可以找到一堆有趣的新mod！",
                "大部分web内容(玩家资料,在线排名等)在游戏内已有原生支持！点点看顶栏上的图标！",
                "右键一个谱面可以选择查看在线信息，隐藏该谱面甚至删除单个难度！",
                "所有删除操作在退出游戏前都是临时的！您可以在“维护”设置中选择恢复被意外删除的内容！",
                "看看多人游戏中的“时移”玩法，他具备房间排行榜和游玩列表的功能！",
                "您可以在游戏中按Ctrl+F11来切换高级fps显示功能！",
                "深入了解性能计数器，并使用Ctrl+F2启用详细的性能记录！",//本句机翻(*懒*)
            };

            return tips[RNG.Next(0, tips.Length)];
        }

        private void animateHeart()
        {
            heart.FlashColour(Color4.White, 750, Easing.OutQuint).Loop();
        }
    }
}