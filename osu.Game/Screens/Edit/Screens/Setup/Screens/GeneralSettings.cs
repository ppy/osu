// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Screens;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class GeneralSettings : EditorSettingsGroup
    {
        protected override string Title => @"general";

        public GeneralSettings()
        {
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
                        CreateSettingLabelText("Artist"),
                        originalArtistName = CreateSettingTextBox("Test artist"),
                        CreateSettingLabelText("Romanised Artist"),
                        romanisedArtistName = CreateSettingTextBox("Test romanised artist"),
                        CreateSettingLabelText("Title"),
                        originalSongTitle = CreateSettingTextBox("Test title"),
                        CreateSettingLabelText("Romanised Title"),
                        romanisedSongTitle = CreateSettingTextBox("Test romanised title"),
                        CreateSettingLabelText("Beatmap Creator"),
                        beatmapCreator = CreateSettingTextBox("Test username"),
                        CreateSettingLabelText("Difficulty"),
                        difficulty = CreateSettingTextBox(/*Beatmap.Value.Metadata.Beatmaps[0].Version*/ "Test version"), // Somehow get the current version of the beatmapset
                        CreateSettingLabelText("Source"),
                        source = CreateSettingTextBox("Test source"),
                        CreateSettingLabelText("Tags"),
                        tags = CreateSettingTextBox("Test tag list"),
                    }
                }
            };
        }

        FocusedTextBox CreateSettingTextBox(string text) => CreateSettingTextBox(text, "test");
        FocusedTextBox CreateSettingTextBox(string text, string placeholderText) => new FocusedTextBox
        {
            Height = 30,
            RelativeSizeAxes = Axes.X,
            Text = text,
            PlaceholderText = placeholderText,
        };
        OsuSpriteText CreateSettingLabelText(string text) => new OsuSpriteText
        {
            Text = text,
            TextSize = 16,
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }
    }
}
