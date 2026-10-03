// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;
using osu.Game.Online.Spectator;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// Contains the basic dependencies of multiplayer test scenes.
    /// </summary>
    public class MultiplayerTestSceneDependencies : OnlinePlayTestSceneDependencies, IMultiplayerTestSceneDependencies
    {
        public TestMultiplayerClient MultiplayerClient { get; }
        public TestSpectatorClient SpectatorClient { get; }
        public QueueController QueueController { get; }

        public MultiplayerTestSceneDependencies()
        {
            MultiplayerClient = new TestMultiplayerClient(RequestsHandler);
            SpectatorClient = CreateSpectatorClient();
            QueueController = new QueueController();

            CacheAs<MultiplayerClient>(MultiplayerClient);
            CacheAs<SpectatorClient>(SpectatorClient);
            CacheAs(QueueController);
        }

        protected virtual TestSpectatorClient CreateSpectatorClient() => new TestSpectatorClient();
    }
}
