// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Components;
using osu.Game.Screens.Multi.Lounge;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimeMultiplayer : Multiplayer
    {
        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            if (client.Room != null)
                client.ChangeState(MultiplayerUserState.Idle).CatchUnobservedExceptions(true);
        }

        protected override void UpdatePollingRate(bool isIdle)
        {
            var timeshiftManager = (RealtimeRoomManager)RoomManager;

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

                    // Don't poll inside the match or anywhere else.
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
            var room = new Room { Name = { Value = $"{API.LocalUser}'s awesome room" } };
            room.Category.Value = RoomCategory.Realtime;
            return room;
        }

        protected override string ScreenTitle => "Multiplayer";

        protected override RoomManager CreateRoomManager() => new RealtimeRoomManager();

        protected override LoungeSubScreen CreateLounge() => new RealtimeLoungeSubScreen();

        protected override OsuButton CreateNewMultiplayerGameButton() => new CreateRealtimeMatchButton();
    }
}
