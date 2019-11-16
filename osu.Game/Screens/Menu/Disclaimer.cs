// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

            textFlow.AddText("这是一个 ", t => t.Font = t.Font.With(Typeface.Exo, 30, FontWeight.Light));
            textFlow.AddText("非常早期的构建版本", t => t.Font = t.Font.With(Typeface.Exo, 30, FontWeight.SemiBold));

            textFlow.AddParagraph("一些事情可能不会按预期工作", t => t.Font = t.Font.With(size: 20));
            textFlow.NewParagraph();

            Action<SpriteText> format = t => t.Font = OsuFont.GetFont(size: 15, weight: FontWeight.SemiBold);

            textFlow.AddParagraph("欢迎来到github的issues页进行反馈", format);
            textFlow.NewParagraph();

            textFlow.AddText("访问 ", format);
            textFlow.AddLink("discord.gg/ppy", "https://discord.gg/ppy", creationParameters: format);
            textFlow.AddText("来一起帮助开发或获取最新进度!", format);

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
                    supportFlow.AddText("考虑成为一名 ", format);
                    supportFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", creationParameters: format);
                    supportFlow.AddText("来帮助我们维护这个游戏", format);
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
