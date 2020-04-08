// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament
{
    public class TournamentGame : TournamentGameBase
    {
        public static ColourInfo GetTeamColour(TeamColour teamColour) => teamColour == TeamColour.Red ? COLOUR_RED : COLOUR_BLUE;

        public static readonly Color4 COLOUR_RED = Color4Extensions.FromHex("#AA1414");
        public static readonly Color4 COLOUR_BLUE = Color4Extensions.FromHex("#1462AA");

        public static readonly Color4 ELEMENT_BACKGROUND_COLOUR = Color4Extensions.FromHex("#fff");
        public static readonly Color4 ELEMENT_FOREGROUND_COLOUR = Color4Extensions.FromHex("#000");

        public static readonly Color4 TEXT_COLOUR = Color4Extensions.FromHex("#fff");

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TournamentSceneManager()
            });

            // we don't want to show the menu cursor as it would appear on stream output.
            MenuCursorContainer.Cursor.Alpha = 0;
        }
    }
}
