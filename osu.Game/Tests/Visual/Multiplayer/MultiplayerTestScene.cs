// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.Rooms;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public abstract class MultiplayerTestScene : OnlinePlayTestScene, IMultiplayerTestDependencies
    {
        public const int PLAYER_1_ID = 55;
        public const int PLAYER_2_ID = 56;

        public TestMultiplayerClient Client => RoomDependencies.Client;
        public new TestMultiplayerRoomManager RoomManager => RoomDependencies.RoomManager;
        public TestUserLookupCache LookupCache => RoomDependencies?.LookupCache;

        protected new MultiplayerRoomTestDependencies RoomDependencies => (MultiplayerRoomTestDependencies)base.RoomDependencies;

        protected override RoomTestDependencies CreateRoomDependencies() => new MultiplayerRoomTestDependencies();

        private readonly bool joinRoom;

        protected MultiplayerTestScene(bool joinRoom = true)
        {
            this.joinRoom = joinRoom;
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            if (joinRoom)
            {
                var room = new Room
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

                RoomManager.CreateRoom(room);
                SelectedRoom.Value = room;
            }
        });

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (joinRoom)
            {
                AddUntilStep("wait for room join", () => Client.Room != null);
            }
        }
    }
}
