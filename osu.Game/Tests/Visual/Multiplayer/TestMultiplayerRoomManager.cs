// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
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
    public class TestMultiplayerRoomManager : MultiplayerRoomManager
    {
        private readonly TestRoomRequestsHandler requestsHandler;

        public TestMultiplayerRoomManager(TestRoomRequestsHandler requestsHandler)
        {
            this.requestsHandler = requestsHandler;
        }

        public IReadOnlyList<Room> ServerSideRooms => requestsHandler.ServerSideRooms;

        /// <summary>
        /// Adds a room to a local "server-side" list that's returned when a <see cref="GetRoomsRequest"/> is fired.
        /// </summary>
        /// <param name="room">The room.</param>
        public void AddServerSideRoom(Room room) => requestsHandler.AddServerSideRoom(room);
    }
}
