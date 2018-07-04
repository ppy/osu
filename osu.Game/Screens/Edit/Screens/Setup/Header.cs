// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Multi.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup
{
    public class Header : Screen
    {
        public const float HEIGHT = 45;

        private readonly OsuSpriteText screenType;
        private readonly Container boxContainer;
        private readonly FillFlowContainer textContainer;
        private readonly FillFlowContainer childModeButtons;

        public Header(Screen initialScreen)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    Height = HEIGHT,
                    RelativeSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = HEIGHT,
                            Colour = OsuColour.FromHex("1c2125"),
                        },
                        new Container
                        {
                            Margin = new MarginPadding { Left = 25, Top = 7.5f },
                            RelativeSizeAxes = Axes.X,
                            Height = HEIGHT,
                            Child = textContainer = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.X,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(7.5f, 0f),
                                Children = new Drawable[]
                                {
                                    new SpriteIcon
                                    {
                                        Size = new Vector2(30),
                                        Icon = FontAwesome.fa_osu_edit_o,
                                    },
                                    new OsuSpriteText
                                    {
                                        Margin = new MarginPadding { Top = 5 },
                                        Text = "Beatmap Setup",
                                        TextSize = 20,
                                    },
                                    screenType = new OsuSpriteText
                                    {
                                        Margin = new MarginPadding { Top = 5 },
                                        Text = "General",
                                        Font = @"Exo2.0-Light",
                                        TextSize = 20,
                                    },
                                },
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            screenType.Colour = colours.BlueDark;
        }

        public void UpdateScreen(string screenName) => screenType.Text = screenName;
    }
}
