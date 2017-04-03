﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Menu
{
    internal class Disclaimer : OsuScreen
    {
        private Intro intro;
        private readonly TextAwesome icon;
        private Color4 iconColour;

        internal override bool ShowOverlays => false;

        internal override bool HasLocalCursorDisplayed => true;

        public Disclaimer()
        {
            ValidForResume = false;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2),
                    Children = new Drawable[]
                    {
                        icon = new TextAwesome
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Icon = FontAwesome.fa_warning,
                            TextSize = 30,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextSize = 30,
                            Text = "This is a development build",
                            Margin = new MarginPadding
                            {
                                Bottom = 20
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Don't expect shit to work perfectly as this is very much a work in progress."
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Don't report bugs because we are aware. Don't complain about missing features because we are adding them."
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Sit back and enjoy. Visit discord.gg/ppy if you want to help out!",
                            Margin = new MarginPadding { Bottom = 20 },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextSize = 12,
                            Text = "oh and yes, you will get seizures.",
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            LoadComponentAsync(intro = new Intro());

            iconColour = colours.Yellow;
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Content.FadeInFromZero(500);

            icon.Delay(1500);
            icon.FadeColour(iconColour, 200);

            Delay(6000, true);

            Content.FadeOut(250);

            Delay(250);

            Schedule(() => Push(intro));
        }
    }
}
