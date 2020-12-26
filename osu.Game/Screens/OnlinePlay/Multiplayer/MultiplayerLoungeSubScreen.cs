// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerLoungeSubScreen : LoungeSubScreen
    {
        protected override FilterControl CreateFilterControl() => new MultiplayerFilterControl();

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new MultiplayerMatchSubScreen(room);

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        public override void Open(Room room)
        {
            if (!client.IsConnected.Value)
            {
                Logger.Log("Not currently connected to the multiplayer server.", LoggingTarget.Runtime, LogLevel.Important);
                return;
            }

            base.Open(room);
        }
    }
}
