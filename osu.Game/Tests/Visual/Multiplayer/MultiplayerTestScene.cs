// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        public new TestMultiplayerRoomManager RoomManager => OnlinePlayDependencies.RoomManager;
        public TestSpectatorClient SpectatorClient => OnlinePlayDependencies.SpectatorClient;

        protected new MultiplayerTestSceneDependencies OnlinePlayDependencies => (MultiplayerTestSceneDependencies)base.OnlinePlayDependencies;

        public bool RoomJoined => MultiplayerClient.RoomJoined;

        private readonly bool joinRoom;

        protected MultiplayerTestScene(bool joinRoom = true)
        {
            this.joinRoom = joinRoom;
        }

        protected virtual Room CreateRoom()
        {
            return new Room
            {
                Name = { Value = "test name" },
                Type = { Value = MatchType.HeadToHead },
                Playlist =
                {
                    new PlaylistItem(new TestBeatmap(Ruleset.Value).BeatmapInfo)
                    {
                        RulesetID = Ruleset.Value.OnlineID
                    }
                }
            };
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (joinRoom)
            {
                AddStep("join room", () =>
                {
                    SelectedRoom.Value = CreateRoom();
                    RoomManager.CreateRoom(SelectedRoom.Value);
                });

                AddUntilStep("wait for room join", () => RoomJoined);
            }
        }

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new MultiplayerTestSceneDependencies();
    }
}
