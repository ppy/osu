// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// The base test scene for all multiplayer components and screens.
    /// </summary>
    public abstract partial class MultiplayerTestScene : OnlinePlayTestScene, IMultiplayerTestSceneDependencies
    {
        public const int PLAYER_1_ID = 55;
        public const int PLAYER_2_ID = 56;

        public TestMultiplayerClient MultiplayerClient => OnlinePlayDependencies.MultiplayerClient;
        public TestSpectatorClient SpectatorClient => OnlinePlayDependencies.SpectatorClient;

        protected new MultiplayerTestSceneDependencies OnlinePlayDependencies => (MultiplayerTestSceneDependencies)base.OnlinePlayDependencies;

        public bool RoomJoined => MultiplayerClient.RoomJoined;

        protected Room CreateDefaultRoom(MatchType type = MatchType.HeadToHead)
        {
            return new Room
            {
                Name = "test name",
                Type = type,
                Playlist =
                [
                    new PlaylistItem(new TestBeatmap(Ruleset.Value).BeatmapInfo)
                    {
                        RulesetID = Ruleset.Value.OnlineID
                    }
                ]
            };
        }

        /// <summary>
        /// Creates and joins a basic multiplayer room.
        /// </summary>
        protected void JoinRoom(Room room) => MultiplayerClient.CreateRoom(room).FireAndForget();

        protected void WaitForJoined() => AddUntilStep("wait for room join", () => RoomJoined);

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new MultiplayerTestSceneDependencies();
    }
}
