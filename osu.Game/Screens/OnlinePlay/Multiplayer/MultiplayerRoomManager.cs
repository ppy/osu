// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Logging;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public partial class MultiplayerRoomManager : RoomManager
    {
        [Resolved]
        private MultiplayerClient multiplayerClient { get; set; }

        public override void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.CreateRoom(room, r => joinMultiplayerRoom(r, r.Password.Value, onSuccess, onError), onError);

        public override void JoinRoom(Room room, string password = null, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            if (!multiplayerClient.IsConnected.Value)
            {
                onError?.Invoke("Not currently connected to the multiplayer server.");
                return;
            }

            // this is done here as a pre-check to avoid clicking on already closed rooms in the lounge from triggering a server join.
            // should probably be done at a higher level, but due to the current structure of things this is the easiest place for now.
            if (room.Status.Value is RoomStatusEnded)
            {
                onError?.Invoke("Cannot join an ended room.");
                return;
            }

            base.JoinRoom(room, password, r => joinMultiplayerRoom(r, password, onSuccess, onError), onError);
        }

        public override void PartRoom()
        {
            if (JoinedRoom.Value == null)
                return;

            base.PartRoom();
            multiplayerClient.LeaveRoom();
        }

        private void joinMultiplayerRoom(Room room, string password, Action<Room> onSuccess = null, Action<string> onError = null)
        {
            Debug.Assert(room.RoomID.Value != null);

            multiplayerClient.JoinRoom(room, password).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    Schedule(() => onSuccess?.Invoke(room));
                else if (t.IsFaulted)
                {
                    const string message = "Failed to join multiplayer room.";

                    if (t.Exception != null)
                        Logger.Error(t.Exception, message);

                    PartRoom();
                    Schedule(() => onError?.Invoke(t.Exception?.AsSingular().Message ?? message));
                }
            });
        }
    }
}
