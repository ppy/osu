// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using osu.Game.Screens.Edit.Screens.Setup.BottomHeaders;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class GeneralScreen : EditorScreen
    {
        private readonly Container content;
        private readonly LabelledTextBox artist;
        private readonly LabelledTextBox romanisedArtist;
        private readonly LabelledTextBox title;
        private readonly LabelledTextBox romanisedTitle;
        private readonly LabelledTextBox beatmapCreator;
        private readonly LabelledTextBox difficulty;
        private readonly LabelledTextBox source;
        private readonly LabelledTextBox tags;

        public string Title => "General";

        public GeneralScreen(WorkingBeatmap workingBeatmap)
        {
            Beatmap.Value = workingBeatmap;

            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = 75, Top = 200 },
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Colour = Color4.White,
                                    Text = "Metadata",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                artist = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    LabelText = "Artist",
                                    TextBoxPlaceholderText = "Artist",
                                    TextBoxText = Beatmap.Value.Metadata.ArtistUnicode,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Metadata.ArtistUnicode = a
                                },
                                romanisedArtist = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Romanised Artist",
                                    TextBoxPlaceholderText = "Romanised Artist",
                                    TextBoxText = Beatmap.Value.Metadata.Artist,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Metadata.Artist = a
                                },
                                title = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    LabelText = "Title",
                                    TextBoxPlaceholderText = "Title",
                                    TextBoxText = Beatmap.Value.Metadata.TitleUnicode,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Metadata.TitleUnicode = a
                                },
                                romanisedTitle = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Romanised Title",
                                    TextBoxPlaceholderText = "Romanised Title",
                                    TextBoxText = Beatmap.Value.Metadata.Title,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Metadata.Title = a
                                },
                                beatmapCreator = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    ReadOnly = true,
                                    LabelText = "Beatmap Creator",
                                    TextBoxPlaceholderText = "Beatmap Creator",
                                    TextBoxText = Beatmap.Value.Metadata.AuthorString
                                },
                                difficulty = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Difficulty",
                                    TextBoxPlaceholderText = "Difficulty",
                                    TextBoxText = Beatmap.Value.Beatmap.BeatmapInfo.Version,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Beatmap.BeatmapInfo.Version = a
                                },
                                source = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Source",
                                    TextBoxPlaceholderText = "Source",
                                    TextBoxText = Beatmap.Value.Metadata.Source,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Metadata.Source = a
                                },
                                tags = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = 150 },
                                    LabelText = "Tags",
                                    TextBoxPlaceholderText = "Tags",
                                    TextBoxText = Beatmap.Value.Metadata.Tags,
                                    TextBoxTextChangedAction = a => Beatmap.Value.Metadata.Tags = a
                                },
                                new GeneralScreenBottomHeader
                                {
                                    Padding = new MarginPadding { Top = 10 },
                                }
                            }
                        }
                    },
                },
            };
        }

        public void ChangeArtist(string newValue) => artist.TextBoxText = newValue;
        public void ChangeRomanisedArtist(string newValue) => romanisedArtist.TextBoxText = newValue;
        public void ChangeTitle(string newValue) => title.TextBoxText = newValue;
        public void ChangeRomanisedTitle(string newValue) => romanisedTitle.TextBoxText = newValue;
        public void ChangeDifficulty(string newValue) => difficulty.TextBoxText = newValue;
        public void ChangeSource(string newValue) => source.TextBoxText = newValue;
        public void ChangeTags(string newValue) => tags.TextBoxText = newValue;
    }
}
