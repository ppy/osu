// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class Playlists : OnlinePlayScreen
    {
        protected override void UpdatePollingRate(bool isIdle)
        {
            var playlistsManager = (PlaylistsRoomManager)RoomManager;

            if (!this.IsCurrentScreen())
            {
                playlistsManager.TimeBetweenListingPolls.Value = 0;
                playlistsManager.TimeBetweenSelectionPolls.Value = 0;
            }
            else
            {
                switch (CurrentSubScreen)
                {
                    case LoungeSubScreen _:
                        playlistsManager.TimeBetweenListingPolls.Value = isIdle ? 120000 : 15000;
                        playlistsManager.TimeBetweenSelectionPolls.Value = isIdle ? 120000 : 15000;
                        break;

                    case RoomSubScreen _:
                        playlistsManager.TimeBetweenListingPolls.Value = 0;
                        playlistsManager.TimeBetweenSelectionPolls.Value = isIdle ? 30000 : 5000;
                        break;

                    default:
                        playlistsManager.TimeBetweenListingPolls.Value = 0;
                        playlistsManager.TimeBetweenSelectionPolls.Value = 0;
                        break;
                }
            }

            Logger.Log($"Polling adjusted (listing: {playlistsManager.TimeBetweenListingPolls.Value}, selection: {playlistsManager.TimeBetweenSelectionPolls.Value})");
        }

        protected override Room CreateNewRoom()
        {
            return new Room { Name = { Value = $"{API.LocalUser}'s awesome playlist" } };
        }

        protected override string ScreenTitle => "Playlists";

        protected override RoomManager CreateRoomManager() => new PlaylistsRoomManager();

        protected override LoungeSubScreen CreateLounge() => new PlaylistsLoungeSubScreen();

        protected override OsuButton CreateNewMultiplayerGameButton() => new CreatePlaylistsRoomButton();
    }
}
