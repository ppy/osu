// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class Multiplayer : OnlinePlayScreen
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
            var multiplayerRoomManager = (MultiplayerRoomManager)RoomManager;

            if (!this.IsCurrentScreen())
            {
                multiplayerRoomManager.TimeBetweenListingPolls.Value = 0;
                multiplayerRoomManager.TimeBetweenSelectionPolls.Value = 0;
            }
            else
            {
                switch (CurrentSubScreen)
                {
                    case LoungeSubScreen _:
                        multiplayerRoomManager.TimeBetweenListingPolls.Value = isIdle ? 120000 : 15000;
                        multiplayerRoomManager.TimeBetweenSelectionPolls.Value = isIdle ? 120000 : 15000;
                        break;

                    // Don't poll inside the match or anywhere else.
                    default:
                        multiplayerRoomManager.TimeBetweenListingPolls.Value = 0;
                        multiplayerRoomManager.TimeBetweenSelectionPolls.Value = 0;
                        break;
                }
            }

            Logger.Log($"Polling adjusted (listing: {multiplayerRoomManager.TimeBetweenListingPolls.Value}, selection: {multiplayerRoomManager.TimeBetweenSelectionPolls.Value})");
        }

        protected override Room CreateNewRoom()
        {
            var room = new Room { Name = { Value = $"{API.LocalUser}'s awesome room" } };
            room.Category.Value = RoomCategory.Realtime;
            return room;
        }

        protected override string ScreenTitle => "Multiplayer";

        protected override RoomManager CreateRoomManager() => new MultiplayerRoomManager();

        protected override LoungeSubScreen CreateLounge() => new MultiplayerLoungeSubScreen();

        protected override OsuButton CreateNewMultiplayerGameButton() => new CreateMultiplayerMatchButton();
    }
}
