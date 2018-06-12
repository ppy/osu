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

        public string Title => "General";

        public GeneralScreen()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    //Colour = OsuColour.FromHex("222d31"),
                    RelativeSizeAxes = Axes.Both,
                    //AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = 75, Top = 200 },
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.Both,
                            //AutoSizeAxes = Axes.X,
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
                                new LabelledTextBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Artist",
                                    TextBoxPlaceholderText = "Artist",
                                    TextBoxText = Beatmap?.Value?.Metadata.ArtistUnicode,
                                    //TextBoxTextChangedAction = a => Beatmap.Value.Metadata.ArtistUnicode = a
                                },
                                new LabelledTextBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Romanised Artist",
                                    TextBoxPlaceholderText = "Romanised Artist",
                                    TextBoxText = Beatmap?.Value?.Metadata.Artist,
                                    //TextBoxTextChangedAction = a => Beatmap.Value.Metadata.Artist = a
                                },
                                new LabelledTextBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Title",
                                    TextBoxPlaceholderText = "Title",
                                    TextBoxText = Beatmap?.Value?.Metadata.TitleUnicode,
                                    //TextBoxTextChangedAction = a => Beatmap.Value.Metadata.TitleUnicode = a
                                },
                                new LabelledTextBox
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Romanised Title",
                                    TextBoxPlaceholderText = "Romanised Title",
                                    TextBoxText = Beatmap?.Value?.Metadata.Title,
                                    //TextBoxTextChangedAction = a => Beatmap.Value.Metadata.Title = a
                                },
                            }
                        }
                    },
                },
            };
        }
    }
}
