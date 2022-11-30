// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// A <see cref="RoomManager"/> for use in multiplayer test scenes.
    /// Should generally not be used by itself outside of a <see cref="MultiplayerTestScene"/>.
    /// </summary>
    public partial class TestMultiplayerRoomManager : MultiplayerRoomManager
    {
        private readonly TestRoomRequestsHandler requestsHandler;

        public TestMultiplayerRoomManager(TestRoomRequestsHandler requestsHandler)
        {
            this.requestsHandler = requestsHandler;
        }

        public IReadOnlyList<Room> ServerSideRooms => requestsHandler.ServerSideRooms;

        public override void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.CreateRoom(room, r => onSuccess?.Invoke(r), onError);

        public override void JoinRoom(Room room, string password = null, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.JoinRoom(room, password, r => onSuccess?.Invoke(r), onError);

        /// <summary>
        /// Adds a room to a local "server-side" list that's returned when a <see cref="GetRoomsRequest"/> is fired.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <param name="host">The host.</param>
        public void AddServerSideRoom(Room room, APIUser host) => requestsHandler.AddServerSideRoom(room, host);
    }
}
