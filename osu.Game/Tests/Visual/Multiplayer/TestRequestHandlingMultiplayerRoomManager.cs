// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// A <see cref="RoomManager"/> for use in multiplayer test scenes, backed by a <see cref="TestRoomRequestsHandler"/>.
    /// Should generally not be used by itself outside of a <see cref="MultiplayerTestScene"/>.
    /// </summary>
    public class TestRequestHandlingMultiplayerRoomManager : MultiplayerRoomManager
    {
        public IReadOnlyList<Room> ServerSideRooms => handler.ServerSideRooms;

        private readonly TestRoomRequestsHandler handler = new TestRoomRequestsHandler();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, OsuGameBase game)
        {
            ((DummyAPIAccess)api).HandleRequest = request => handler.HandleRequest(request, api.LocalUser.Value, game);
        }

        /// <summary>
        /// Adds a room to a local "server-side" list that's returned when a <see cref="GetRoomsRequest"/> is fired.
        /// </summary>
        /// <param name="room">The room.</param>
        public void AddServerSideRoom(Room room) => handler.AddServerSideRoom(room);
    }
}
