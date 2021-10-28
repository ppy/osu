// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.Rooms;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// The base test scene for all multiplayer components and screens.
    /// </summary>
    public abstract class MultiplayerTestScene : OnlinePlayTestScene, IMultiplayerTestSceneDependencies
    {
        public const int PLAYER_1_ID = 55;
        public const int PLAYER_2_ID = 56;

        public TestMultiplayerClient Client => OnlinePlayDependencies.Client;
        public new TestMultiplayerRoomManager RoomManager => OnlinePlayDependencies.RoomManager;
        public TestUserLookupCache LookupCache => OnlinePlayDependencies?.LookupCache;
        public TestSpectatorClient SpectatorClient => OnlinePlayDependencies?.SpectatorClient;

        protected new MultiplayerTestSceneDependencies OnlinePlayDependencies => (MultiplayerTestSceneDependencies)base.OnlinePlayDependencies;

        private readonly bool joinRoom;

        protected MultiplayerTestScene(bool joinRoom = true)
        {
            this.joinRoom = joinRoom;
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            if (joinRoom)
                SelectedRoom.Value = CreateRoom();
        });

        protected virtual Room CreateRoom()
        {
            return new Room
            {
                Name = { Value = "test name" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = new TestBeatmap(Ruleset.Value).BeatmapInfo },
                        Ruleset = { Value = Ruleset.Value }
                    }
                }
            };
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (joinRoom)
            {
                AddStep("join room", () => RoomManager.CreateRoom(SelectedRoom.Value));
                AddUntilStep("wait for room join", () => Client.Room != null);
            }
        }

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new MultiplayerTestSceneDependencies();
    }
}
