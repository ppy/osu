// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Footer;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match
{
    public partial class ScreenMatchmaking
    {
        private partial class HistoryFooterButton : ScreenFooterButton
        {
            [Resolved]
            private OsuGame? game { get; set; }

            private readonly MultiplayerRoom room;

            public HistoryFooterButton(MultiplayerRoom room)
            {
                this.room = room;

                Action = openRoomHistory;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Text = "History";
                Icon = FontAwesome.Solid.Globe;
                AccentColour = colours.Lime1;
            }

            private void openRoomHistory()
                => game?.OpenUrlExternally($@"/multiplayer/rooms/{room.RoomID}/events");
        }
    }
}
