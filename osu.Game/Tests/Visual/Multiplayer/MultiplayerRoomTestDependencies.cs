// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class MultiplayerRoomTestDependencies : RoomTestDependencies, IMultiplayerRoomTestDependencies
    {
        public TestMultiplayerClient Client { get; }
        public new TestMultiplayerRoomManager RoomManager => (TestMultiplayerRoomManager)base.RoomManager;

        public MultiplayerRoomTestDependencies()
        {
            Client = new TestMultiplayerClient(RoomManager);
            CacheAs<MultiplayerClient>(Client);
        }

        protected override IRoomManager CreateRoomManager() => new TestMultiplayerRoomManager();
    }
}
