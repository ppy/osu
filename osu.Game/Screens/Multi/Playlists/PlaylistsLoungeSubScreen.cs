// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Screens.Multi.Match;

namespace osu.Game.Screens.Multi.Playlists
{
    public class PlaylistsLoungeSubScreen : LoungeSubScreen
    {
        protected override FilterControl CreateFilterControl() => new PlaylistsFilterControl();

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new PlaylistsRoomSubScreen(room);
    }
}
