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
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                    Size = new Vector2(icon_size),
                    Y = icon_y,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Y = icon_y + icon_size,
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

            textFlow.AddText("注意!这是一个", t => t.Font = t.Font.With(Typeface.Exo, 35, FontWeight.Light));
            textFlow.AddText("早期开发版本", t => t.Font = t.Font.With(Typeface.Exo, 35, FontWeight.SemiBold));

            textFlow.AddParagraph("一些功能可能不会像预期的那样工作", t => t.Font = t.Font.With(size: 25));
            textFlow.NewParagraph();

            static void format(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold);

            textFlow.AddParagraph("欢迎前往", format);
            textFlow.AddLink("github页面", "https://github.com/ppy/osu/issues", creationParameters: format);
            textFlow.AddText("来提交问题报告", format);
            textFlow.NewParagraph();
            textFlow.NewParagraph();

            textFlow.AddText("或者加入", format);
            textFlow.AddLink("discord.gg/ppy", "https://discord.gg/ppy", creationParameters: format);
            textFlow.AddText("来帮助开发或者关注进度!", format);

            textFlow.AddParagraph("反馈该版本的翻译问题,请前往", format);
            textFlow.AddLink("mfosu的github页面", "https://github.com/MATRIX-feather/osu/issues", creationParameters: format);
            textFlow.AddText("来提交问题报告", format);
            textFlow.NewParagraph();

            textFlow.NewParagraph();
            textFlow.NewParagraph();
            textFlow.NewParagraph();

            iconColour = colours.Yellow;

            currentUser.BindTo(api.LocalUser);
            currentUser.BindValueChanged(e =>
            {
                supportFlow.Children.ForEach(d => d.FadeOut().Expire());

                if (e.NewValue.IsSupporter)
                {
                    supportFlow.AddText("感谢支持osu!", format);
                }
                else
                {
                    supportFlow.AddText("您也可以考虑成为一名", format);
                    supportFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", creationParameters: format);
                    supportFlow.AddText("来支持游戏的开发", format);
                }

                heart = supportFlow.AddIcon(FontAwesome.Solid.Heart, t =>
                {
                    t.Padding = new MarginPadding { Left = 5 };
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

        private void animateHeart()
        {
            heart.FlashColour(Color4.White, 750, Easing.OutQuint).Loop();
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

            icon.Delay(1000).FadeColour(iconColour, 200, Easing.OutQuint);
            icon.Delay(1000)
                .MoveToY(icon_y * 1.1f, 160, Easing.OutCirc)
                .RotateTo(-10, 160, Easing.OutCirc)
                .Then()
                .MoveToY(icon_y, 160, Easing.InCirc)
                .RotateTo(0, 160, Easing.InCirc);

            supportFlow.FadeOut().Delay(2000).FadeIn(500);

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
    }
}
