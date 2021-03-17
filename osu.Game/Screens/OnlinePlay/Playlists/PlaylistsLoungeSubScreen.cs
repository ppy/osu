// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsLoungeSubScreen : LoungeSubScreen
    {
        protected override FilterControl CreateFilterControl() => new PlaylistsFilterControl();

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new PlaylistsRoomSubScreen(room);
    }
}
