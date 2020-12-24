// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Match;

namespace osu.Game.Screens.Multi.Timeshift
{
    public class TimeshiftMultiplayer : Multiplayer
    {
        protected override void UpdatePollingRate(bool isIdle)
        {
            var timeshiftManager = (TimeshiftRoomManager)RoomManager;

            if (!this.IsCurrentScreen())
            {
                timeshiftManager.TimeBetweenListingPolls.Value = 0;
                timeshiftManager.TimeBetweenSelectionPolls.Value = 0;
            }
            else
            {
                switch (CurrentSubScreen)
                {
                    case LoungeSubScreen _:
                        timeshiftManager.TimeBetweenListingPolls.Value = isIdle ? 120000 : 15000;
                        timeshiftManager.TimeBetweenSelectionPolls.Value = isIdle ? 120000 : 15000;
                        break;

                    case RoomSubScreen _:
                        timeshiftManager.TimeBetweenListingPolls.Value = 0;
                        timeshiftManager.TimeBetweenSelectionPolls.Value = isIdle ? 30000 : 5000;
                        break;

                    default:
                        timeshiftManager.TimeBetweenListingPolls.Value = 0;
                        timeshiftManager.TimeBetweenSelectionPolls.Value = 0;
                        break;
                }
            }

            Logger.Log($"Polling adjusted (listing: {timeshiftManager.TimeBetweenListingPolls.Value}, selection: {timeshiftManager.TimeBetweenSelectionPolls.Value})");
        }

        protected override Room CreateNewRoom()
        {
            return new Room { Name = { Value = $"{API.LocalUser}'s awesome playlist" } };
        }

        protected override string ScreenTitle => "Playlists";

        protected override RoomManager CreateRoomManager() => new TimeshiftRoomManager();

        protected override LoungeSubScreen CreateLounge() => new TimeshiftLoungeSubScreen();

        protected override OsuButton CreateNewMultiplayerGameButton() => new CreateTimeshiftRoomButton();
    }
}
