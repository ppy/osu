// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// Contains the basic dependencies of multiplayer test scenes.
    /// </summary>
    public class MultiplayerTestSceneDependencies : OnlinePlayTestSceneDependencies, IMultiplayerTestSceneDependencies
    {
        public TestMultiplayerClient Client { get; }
        public TestUserLookupCache LookupCache { get; }
        public TestSpectatorClient SpectatorClient { get; }
        public new TestMultiplayerRoomManager RoomManager => (TestMultiplayerRoomManager)base.RoomManager;

        public MultiplayerTestSceneDependencies()
        {
            Client = new TestMultiplayerClient(RoomManager);
            LookupCache = new TestUserLookupCache();
            SpectatorClient = CreateSpectatorClient();

            CacheAs<MultiplayerClient>(Client);
            CacheAs<UserLookupCache>(LookupCache);
            CacheAs<SpectatorClient>(SpectatorClient);
        }

        protected override IRoomManager CreateRoomManager() => new TestMultiplayerRoomManager(RequestsHandler);

        protected virtual TestSpectatorClient CreateSpectatorClient() => new TestSpectatorClient();
    }
}
