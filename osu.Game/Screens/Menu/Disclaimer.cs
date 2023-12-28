// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
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
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class Disclaimer : StartupScreen
    {
        private SpriteIcon icon;
        private Color4 iconColour;
        private LinkFlowContainer textFlow;
        private LinkFlowContainer supportFlow;

        private Drawable heart;

        private const float icon_y = -85;
        private const float icon_size = 30;

        private readonly OsuScreen nextScreen;

        private readonly Bindable<APIUser> currentUser = new Bindable<APIUser>();
        private FillFlowContainer fill;

        private readonly List<ITextPart> expendableText = new List<ITextPart>();

        public Disclaimer(OsuScreen nextScreen = null)
        {
            this.nextScreen = nextScreen;
            ValidForResume = false;
        }

        [Resolved]
        private IAPIProvider api { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
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

            textFlow.AddText("this is osu!", t => t.Font = t.Font.With(Typeface.Torus, 30, FontWeight.Regular));

            expendableText.Add(textFlow.AddText("lazer", t =>
            {
                t.Font = t.Font.With(Typeface.Torus, 30, FontWeight.Regular);
                t.Colour = colours.PinkLight;
            }));

            static void formatRegular(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.Regular);
            static void formatSemiBold(SpriteText t) => t.Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold);

            textFlow.NewParagraph();

            textFlow.AddText("the next ", formatRegular);
            textFlow.AddText("major update", t =>
            {
                t.Font = t.Font.With(Typeface.Torus, 20, FontWeight.SemiBold);
                t.Colour = colours.Pink;
            });
            expendableText.Add(textFlow.AddText(" coming to osu!", formatRegular));
            textFlow.AddText(".", formatRegular);

            textFlow.NewParagraph();
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
                    supportFlow.AddText("Eternal thanks to you for supporting osu!", formatSemiBold);
                }
                else
                {
                    supportFlow.AddText("Consider becoming an ", formatSemiBold);
                    supportFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", formatSemiBold);
                    supportFlow.AddText(" to help support osu!'s development", formatSemiBold);
                }

                supportFlow.AddIcon(FontAwesome.Solid.Heart, t =>
                {
                    heart = t;

                    t.Padding = new MarginPadding { Left = 5, Top = 3 };
                    t.Font = t.Font.With(size: 20);
                    t.Origin = Anchor.Centre;
                    t.Colour = colours.Pink;

                    Schedule(() => heart?.FlashColour(Color4.White, 750, Easing.OutQuint).Loop());
                });

                if (supportFlow.IsPresent)
                    supportFlow.FadeInFromZero(500);
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (nextScreen != null)
                LoadComponentAsync(nextScreen);

            ((IBindable<APIUser>)currentUser).BindTo(api.LocalUser);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            // Once this screen has finished being displayed, we don't want to unnecessarily handle user change events.
            currentUser.UnbindAll();
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            icon.RotateTo(10);
            icon.FadeOut();
            icon.ScaleTo(0.5f);

            icon.Delay(500).FadeIn(500).ScaleTo(1, 500, Easing.OutQuint);

            using (BeginDelayedSequence(3000))
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
                    Schedule(() => expendableText.SelectMany(t => t.Drawables).ForEach(t =>
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

            this
                .FadeInFromZero(500)
                .Then(5500)
                .FadeOut(250)
                .ScaleTo(0.9f, 250, Easing.InQuint)
                .Finally(_ =>
                {
                    if (nextScreen != null)
                        this.Push(nextScreen);
                });
        }
    }
}
