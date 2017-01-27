//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Input;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using osu.Game.Overlays.PopUpDialogs;
using osu.Game.Screens.Select;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    class TestCaseSongSelectOptions : TestCase
    {
        public override string Name => @"SongSelectOptions";

        public override string Description => @"Test the Song Select Options container + buttons";

        private SongSelectOptionsContainer songSelectOptions;
        private const float options_button_width = 140;
        private const float options_button_height = 125;

        public override void Reset()
        {
            base.Reset();

            Children = new Drawable[]
            {
                songSelectOptions = new SongSelectOptionsContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new SongSelectOptionsButton[]
                    {
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_osu_cross_o,
                            TextLineA = "Remove",
                            TextLineB = "from Unplayed",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(103, 83, 196, 255),
                        },
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_eraser,
                            TextLineA = "Clear",
                            TextLineB = "local scores",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(103, 83, 196, 255),
                        },
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_pencil,
                            TextLineA = "Edit",
                            TextLineB = "Beatmap",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(227, 159, 12, 255),
                        },
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_trash,
                            TextLineA = "Delete",
                            TextLineB = "Beatmap",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(238, 51, 153, 255),
                        },
                    }
                }
            };
            AddButton(@"Toggle Options", ToggleOptions);
        }

        public void ToggleOptions()
        {
            if (songSelectOptions.State == SongSelectOptionsState.Hidden)
                songSelectOptions.State = SongSelectOptionsState.Visible;
            else
                songSelectOptions.State = SongSelectOptionsState.Hidden;
        }
    }
}