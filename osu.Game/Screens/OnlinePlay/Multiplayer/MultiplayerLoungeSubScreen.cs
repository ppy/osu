// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MultiplayerClient client { get; set; }

        protected override FilterCriteria CreateFilterCriteria()
        {
            var criteria = base.CreateFilterCriteria();
            criteria.Category = @"realtime";
            return criteria;
        }

        protected override OsuButton CreateNewRoomButton() => new CreateMultiplayerMatchButton();

        protected override Room CreateNewRoom() => new Room
        {
            Name = { Value = $"{api.LocalUser}'s awesome room" },
            Category = { Value = RoomCategory.Realtime },
            Type = { Value = MatchType.HeadToHead },
        };

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new MultiplayerMatchSubScreen(room);

        protected override void OpenNewRoom(Room room)
        {
            if (client?.IsConnected.Value != true)
            {
                Logger.Log("Not currently connected to the multiplayer server.", LoggingTarget.Runtime, LogLevel.Important);
                return;
            }

            base.OpenNewRoom(room);
        }
    }
}
