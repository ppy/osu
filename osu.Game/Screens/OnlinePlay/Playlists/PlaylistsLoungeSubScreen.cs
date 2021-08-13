// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        protected override FilterControl CreateFilterControl() => new PlaylistsFilterControl();

        protected override OsuButton CreateNewRoomButton() => new CreatePlaylistsRoomButton();

        protected override Room CreateNewRoom()
        {
            return new Room
            {
                Name = { Value = $"{api.LocalUser}'s awesome playlist" },
                Type = { Value = MatchType.Playlists }
            };
        }

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new PlaylistsRoomSubScreen(room);
    }
}
