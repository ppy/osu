// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Edit.Screens.Setup.BottomHeaders;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class GeneralScreen : EditorScreen
    {
        private readonly LabelledTextBox artist;
        private readonly LabelledTextBox romanisedArtist;
        private readonly LabelledTextBox title;
        private readonly LabelledTextBox romanisedTitle;
        private readonly LabelledTextBox beatmapCreator;
        private readonly LabelledTextBox difficulty;
        private readonly LabelledTextBox source;
        private readonly LabelledTextBox tags;

        public string Title => "General";

        public GeneralScreen()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = Setup.SCREEN_LEFT_PADDING, Top = Setup.SCREEN_TOP_PADDING },
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
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Artist",
                                    TextBoxPlaceholderText = "Artist",
                                },
                                romanisedArtist = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Romanised Artist",
                                    TextBoxPlaceholderText = "Romanised Artist",
                                },
                                title = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Title",
                                    TextBoxPlaceholderText = "Title",
                                },
                                romanisedTitle = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Romanised Title",
                                    TextBoxPlaceholderText = "Romanised Title",
                                },
                                beatmapCreator = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = Setup.SCREEN_RIGHT_PADDING },
                                    ReadOnly = true,
                                    LabelText = "Beatmap Creator",
                                    TextBoxPlaceholderText = "Beatmap Creator",
                                },
                                difficulty = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Difficulty",
                                    TextBoxPlaceholderText = "Difficulty",
                                },
                                source = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Source",
                                    TextBoxPlaceholderText = "Source",
                                },
                                tags = new LabelledTextBox
                                {
                                    Padding = new MarginPadding { Right = Setup.SCREEN_RIGHT_PADDING },
                                    LabelText = "Tags",
                                    TextBoxPlaceholderText = "Tags",
                                }
                            }
                        },
                        new GeneralScreenBottomHeader
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = Setup.SCREEN_LEFT_PADDING, Top = Setup.SCREEN_BOTTOM_HEADER_TOP_PADDING },
                        }
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            artist.TextBoxTextChanged += a => Beatmap.Value.Metadata.ArtistUnicode = a;
            romanisedArtist.TextBoxTextChanged += a => Beatmap.Value.Metadata.Artist = a;
            title.TextBoxTextChanged += a => Beatmap.Value.Metadata.TitleUnicode = a;
            romanisedTitle.TextBoxTextChanged += a => Beatmap.Value.Metadata.Title = a;
            difficulty.TextBoxTextChanged += a => Beatmap.Value.Beatmap.BeatmapInfo.Version = a;
            source.TextBoxTextChanged += a => Beatmap.Value.Metadata.Source = a;
            tags.TextBoxTextChanged += a => Beatmap.Value.Metadata.Tags = a;
        }

        public void ChangeArtist(string newValue) => artist.TextBoxText = newValue;
        public void ChangeRomanisedArtist(string newValue) => romanisedArtist.TextBoxText = newValue;
        public void ChangeTitle(string newValue) => title.TextBoxText = newValue;
        public void ChangeRomanisedTitle(string newValue) => romanisedTitle.TextBoxText = newValue;
        public void ChangeDifficulty(string newValue) => difficulty.TextBoxText = newValue;
        public void ChangeSource(string newValue) => source.TextBoxText = newValue;
        public void ChangeTags(string newValue) => tags.TextBoxText = newValue;

        private void updateInfo()
        {
            artist.TextBoxText = Beatmap.Value?.Metadata.ArtistUnicode;
            romanisedArtist.TextBoxText = Beatmap.Value?.Metadata.Artist;
            title.TextBoxText = Beatmap.Value?.Metadata.TitleUnicode;
            romanisedTitle.TextBoxText = Beatmap.Value?.Metadata.Title;
            beatmapCreator.TextBoxText = Beatmap.Value?.Metadata.AuthorString;
            difficulty.TextBoxText = Beatmap.Value?.Beatmap.BeatmapInfo.Version;
            source.TextBoxText = Beatmap.Value?.Metadata.Source;
            tags.TextBoxText = Beatmap.Value?.Metadata.Tags;
        }
    }
}
