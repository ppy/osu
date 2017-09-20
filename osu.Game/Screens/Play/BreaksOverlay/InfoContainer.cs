// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class InfoContainer : FillFlowContainer
    {
        public InfoContainer()
        {
            AutoSizeAxes = Axes.Both;
            Alpha = 0;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(5);
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"current progress".ToUpper(),
                    TextSize = 15,
                    Font = "Exo2.0-Black",
                },
                new FillFlowContainer<InfoLine>
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Direction = FillDirection.Vertical,
                    Children = new []
                    {
                        new InfoLine(@"Accuracy", @"-"),
                        new InfoLine(@"Rank", @"-"),
                        new InfoLine(@"Grade", @"-"),
                    },
                }
            };
        }

        private class InfoLine : Container
        {
            private const int margin = 2;

            private readonly OsuSpriteText text;
            private readonly OsuSpriteText valueText;

            public InfoLine(string name, string value)
            {
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    text = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreRight,
                        Text = name,
                        TextSize = 17,
                        Margin = new MarginPadding { Right = margin }
                    },
                    valueText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.CentreLeft,
                        Text = value,
                        TextSize = 17,
                        Font = "Exo2.0-Bold",
                        Margin = new MarginPadding { Left = margin }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                text.Colour = colours.Yellow;
                valueText.Colour = colours.YellowLight;
            }
        }
    }
}
