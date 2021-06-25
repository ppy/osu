// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class MultiplayerRoomTestDependencies : RoomTestDependencies, IMultiplayerTestDependencies
    {
        public TestMultiplayerClient Client { get; }
        public TestUserLookupCache LookupCache { get; }
        public new TestMultiplayerRoomManager RoomManager => (TestMultiplayerRoomManager)base.RoomManager;

        public MultiplayerRoomTestDependencies()
        {
            Client = new TestMultiplayerClient(RoomManager);
            LookupCache = new TestUserLookupCache();

            CacheAs<MultiplayerClient>(Client);
            CacheAs<UserLookupCache>(LookupCache);
        }

        protected override IRoomManager CreateRoomManager() => new TestMultiplayerRoomManager();
    }
}
