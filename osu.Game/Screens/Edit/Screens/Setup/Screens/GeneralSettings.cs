// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class GeneralSettings : SettingsGroup
    {
        protected override string Title => @"general";

        public GeneralSettings()
        {
            AllowCollapsing = false;

            FillFlowContainer settingsContainer;
            FocusedTextBox originalArtistName;
            FocusedTextBox romanisedArtistName;
            FocusedTextBox originalSongTitle;
            FocusedTextBox romanisedSongTitle;
            FocusedTextBox beatmapCreator;
            FocusedTextBox difficulty;
            FocusedTextBox source;
            FocusedTextBox tags;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        createSettingLabelText("Artist"),
                        originalArtistName = createSettingTextBox("Test artist"),
                        createSettingLabelText("Romanised Artist"),
                        romanisedArtistName = createSettingTextBox("Test romanised artist"),
                        createSettingLabelText("Title"),
                        originalSongTitle = createSettingTextBox("Test title"),
                        createSettingLabelText("Romanised Title"),
                        romanisedSongTitle = createSettingTextBox("Test romanised title"),
                        createSettingLabelText("Beatmap Creator"),
                        beatmapCreator = createSettingTextBox("Test username"),
                        createSettingLabelText("Difficulty"),
                        difficulty = createSettingTextBox(/*Beatmap.Value.Metadata.Beatmaps[0].Version*/ "Test version"), // Somehow get the current version of the beatmapset
                        createSettingLabelText("Source"),
                        source = createSettingTextBox("Test source"),
                        createSettingLabelText("Tags"),
                        tags = createSettingTextBox("Test tag list"),
                    }
                }
            };
            // Currently only assigned for AppVeyor to not complain for it being unused
            originalArtistName.Current.ValueChanged += a => { };
            romanisedArtistName.Current.ValueChanged += a => { };
            originalSongTitle.Current.ValueChanged += a => { };
            romanisedSongTitle.Current.ValueChanged += a => { };
            beatmapCreator.Current.ValueChanged += a => { };
            difficulty.Current.ValueChanged += a => { };
            source.Current.ValueChanged += a => { };
            tags.Current.ValueChanged += a => { };
        }

        private FocusedTextBox createSettingTextBox(string text) => createSettingTextBox(text, "test");
        private FocusedTextBox createSettingTextBox(string text, string placeholderText) => new FocusedTextBox
        {
            Height = 30,
            RelativeSizeAxes = Axes.X,
            Text = text,
            PlaceholderText = placeholderText,
        };
        private OsuSpriteText createSettingLabelText(string text) => new OsuSpriteText
        {
            Text = text,
            TextSize = 16,
        };
    }
}
