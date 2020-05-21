// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament
{
    public class TournamentGame : TournamentGameBase
    {
        public static ColourInfo GetTeamColour(TeamColour teamColour) => teamColour == TeamColour.Red ? COLOUR_RED : COLOUR_BLUE;

        public static readonly Colour4 COLOUR_RED = Colour4.FromHex("#AA1414");
        public static readonly Colour4 COLOUR_BLUE = Colour4.FromHex("#1462AA");

        public static readonly Colour4 ELEMENT_BACKGROUND_COLOUR = Colour4.FromHex("#fff");
        public static readonly Colour4 ELEMENT_FOREGROUND_COLOUR = Colour4.FromHex("#000");

        public static readonly Colour4 TEXT_COLOUR = Colour4.FromHex("#fff");

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
