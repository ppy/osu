// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Screens.Menu
{
    public class Disclaimer : OsuScreen
    {
        private Intro intro;
        private SpriteIcon icon;
        private Color4 iconColour;
        private LinkFlowContainer textFlow;

        protected override bool HideOverlaysOnEnter => true;
        protected override OverlayActivation InitialOverlayActivationMode => OverlayActivation.Disabled;

        public override bool CursorVisible => false;

        private const float icon_y = -0.09f;

        public Disclaimer()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_warning,
                    Size = new Vector2(30),
                    RelativePositionAxes = Axes.Both,
                    Y = icon_y,
                },
                textFlow = new LinkFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(50),
                    TextAnchor = Anchor.TopCentre,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
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

            textFlow.AddParagraph("Don't expect everything to work perfectly.");
            textFlow.AddParagraph("");
            textFlow.AddParagraph("Detailed bug reports are welcomed via github issues.");
            textFlow.AddParagraph("");

            textFlow.AddText("Visit ");
            textFlow.AddLink("discord.gg/ppy", "https://discord.gg/ppy");
            textFlow.AddText(" if you want to help out or follow progress!");

            iconColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            LoadComponentAsync(intro = new Intro());
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            icon.Delay(1500).FadeColour(iconColour, 200, Easing.OutQuint);
            icon.Delay(1500).MoveToY(icon_y * 1.1f, 100, Easing.OutCirc).Then().MoveToY(icon_y, 100, Easing.InCirc);

            Content
                .FadeInFromZero(500)
                .Then(5500)
                .FadeOut(250)
                .ScaleTo(0.9f, 250, Easing.InQuint)
                .Finally(d => Push(intro));
        }
    }
}
