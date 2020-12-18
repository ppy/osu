// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimeRoomManager : RoomManager
    {
        [Resolved]
        private StatefulMultiplayerClient multiplayerClient { get; set; }

        public override void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.CreateRoom(room, r => joinMultiplayerRoom(r, onSuccess), onError);

        public override void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.JoinRoom(room, r => joinMultiplayerRoom(r, onSuccess), onError);

        private void joinMultiplayerRoom(Room room, Action<Room> onSuccess = null)
        {
            Debug.Assert(room.RoomID.Value != null);

            var joinTask = multiplayerClient.JoinRoom(room);
            joinTask.ContinueWith(_ => onSuccess?.Invoke(room));
            joinTask.ContinueWith(t =>
            {
                PartRoom();
                if (t.Exception != null)
                    Logger.Error(t.Exception, "Failed to join multiplayer room.");
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        protected override RoomPollingComponent[] CreatePollingComponents() => new RoomPollingComponent[]
        {
            new ListingPollingComponent()
        };
    }
}
