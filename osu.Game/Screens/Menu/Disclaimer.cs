// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Screens.Menu
{
    public class Disclaimer : OsuScreen
    {
        private Intro intro;
        private SpriteIcon icon;
        private Color4 iconColour;
        private LinkFlowContainer textFlow;

        public override bool HideOverlaysOnEnter => true;
        public override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        public override bool CursorVisible => false;

        private readonly List<Drawable> supporterDrawables = new List<Drawable>();
        private Drawable heart;

        private const float icon_y = -85;

        public Disclaimer()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_warning,
                    Size = new Vector2(30),
                    Y = icon_y,
                },
                textFlow = new LinkFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(50),
                    TextAnchor = Anchor.TopCentre,
                    Y = -110,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    Spacing = new Vector2(0, 2),
                }
            };

            textFlow.AddText("This is an ", t =>
            {
                t.TextSize = 30;
                t.Font = @"Exo2.0-Light";
            });
            textFlow.AddText("early development build", t =>
            {
                t.TextSize = 30;
                t.Font = @"Exo2.0-SemiBold";
            });

            textFlow.AddParagraph("Things may not work as expected", t => t.TextSize = 20);
            textFlow.NewParagraph();

            Action<SpriteText> format = t =>
            {
                t.TextSize = 15;
                t.Font = @"Exo2.0-SemiBold";
            };

            textFlow.AddParagraph("Detailed bug reports are welcomed via github issues.", format);
            textFlow.NewParagraph();

            textFlow.AddText("Visit ", format);
            textFlow.AddLink("discord.gg/ppy", "https://discord.gg/ppy", creationParameters:format);
            textFlow.AddText(" to help out or follow progress!", format);

            textFlow.NewParagraph();
            textFlow.NewParagraph();
            textFlow.NewParagraph();

            supporterDrawables.AddRange(textFlow.AddText("Consider becoming an ", format));
            supporterDrawables.AddRange(textFlow.AddLink("osu!supporter", "https://osu.ppy.sh/home/support", creationParameters: format));
            supporterDrawables.AddRange(textFlow.AddText(" to help support the game", format));

            supporterDrawables.Add(heart = textFlow.AddIcon(FontAwesome.fa_heart, t =>
            {
                t.Padding = new MarginPadding { Left = 5 };
                t.TextSize = 12;
                t.Colour = colours.Pink;
                t.Origin = Anchor.Centre;
            }).First());

            iconColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            LoadComponentAsync(intro = new Intro());
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

            supporterDrawables.ForEach(d => d.FadeOut().Delay(2000).FadeIn(500));

            this
                .FadeInFromZero(500)
                .Then(5500)
                .FadeOut(250)
                .ScaleTo(0.9f, 250, Easing.InQuint)
                .Finally(d => this.Push(intro));

            heart.FlashColour(Color4.White, 750, Easing.OutQuint).Loop();
        }
    }
}
