// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class GeneralScreen : EditorScreen
    {
        private readonly Container content;

        public override string Title => "General";

        public GeneralScreen()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    //Colour = OsuColour.FromHex("222d31"),
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = 75, Top = 200 },
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Colour = Color4.White,
                                    Text = "Metadata",
                                    TextSize = 18,
                                    Font = @"Exo2.0-Bold",
                                },
                                new Container
                                {
                                    Colour = OsuColour.FromHex("1c2125"),
                                    //Anchor = Anchor.CentreLeft,
                                    //Origin = Anchor.CentreLeft,
                                    //AutoSizeAxes = Axes.X,
                                    //RelativeSizeAxes = Axes.Y,
                                    //Height = 50,
                                    //Size = new Vector2(50),
                                    Children = new Drawable[]
                                    {
                                        new OsuTextBox
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            //Size = new Vector2(50),
                                            //Height = 50,
                                            Padding = new MarginPadding { Left = 300 },
                                        },
                                        new OsuSpriteText
                                        {
                                            Padding = new MarginPadding { Left = 20 },
                                            Colour = Color4.White,
                                            TextSize = 16,
                                            Text = "Artist",
                                            Font = @"Exo2.0-Bold",
                                        },
                                    }
                                },
                                new LabelledTextBox
                                {
                                    LabelText = "Artist",
                                },
                                new LabelledTextBox
                                {
                                    LabelText = "Romanised Artist",
                                },
                                new LabelledTextBox
                                {
                                    LabelText = "Title",
                                },
                                new LabelledTextBox
                                {
                                    LabelText = "Romanised Title",
                                },
                            }
                        }
                    },
                },
            };
        }
    }
}
