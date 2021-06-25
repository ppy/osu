// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [HeadlessTest]
    public class TestSceneMultiplayerRoomManager : MultiplayerTestScene
    {
        protected override OnlinePlayTestDependencies CreateOnlinePlayDependencies() => new TestDependencies();

        public TestSceneMultiplayerRoomManager()
            : base(false)
        {
        }

        [Test]
        public void TestPollsInitially()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                RoomManager.CreateRoom(createRoom(r => r.Name.Value = "1"));
                RoomManager.PartRoom();
                RoomManager.CreateRoom(createRoom(r => r.Name.Value = "2"));
                RoomManager.PartRoom();
                RoomManager.ClearRooms();
            });

            AddAssert("manager polled for rooms", () => ((RoomManager)RoomManager).Rooms.Count == 2);
            AddAssert("initial rooms received", () => RoomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsClearedOnDisconnection()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                RoomManager.CreateRoom(createRoom());
                RoomManager.PartRoom();
                RoomManager.CreateRoom(createRoom());
                RoomManager.PartRoom();
            });

            AddStep("disconnect", () => Client.Disconnect());

            AddAssert("rooms cleared", () => ((RoomManager)RoomManager).Rooms.Count == 0);
            AddAssert("initial rooms not received", () => !RoomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsPolledOnReconnect()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                RoomManager.CreateRoom(createRoom());
                RoomManager.PartRoom();
                RoomManager.CreateRoom(createRoom());
                RoomManager.PartRoom();
            });

            AddStep("disconnect", () => Client.Disconnect());
            AddStep("connect", () => Client.Connect());

            AddAssert("manager polled for rooms", () => ((RoomManager)RoomManager).Rooms.Count == 2);
            AddAssert("initial rooms received", () => RoomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsNotPolledWhenJoined()
        {
            AddStep("create room manager with a room", () =>
            {
                RoomManager.CreateRoom(createRoom());
                RoomManager.ClearRooms();
            });

            AddAssert("manager not polled for rooms", () => ((RoomManager)RoomManager).Rooms.Count == 0);
            AddAssert("initial rooms not received", () => !RoomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestMultiplayerRoomJoinedWhenCreated()
        {
            AddStep("create room manager with a room", () =>
            {
                RoomManager.CreateRoom(createRoom());
            });

            AddUntilStep("multiplayer room joined", () => Client.Room != null);
        }

        [Test]
        public void TestMultiplayerRoomPartedWhenAPIRoomParted()
        {
            AddStep("create room manager with a room", () =>
            {
                RoomManager.CreateRoom(createRoom());
                RoomManager.PartRoom();
            });

            AddAssert("multiplayer room parted", () => Client.Room == null);
        }

        [Test]
        public void TestMultiplayerRoomJoinedWhenAPIRoomJoined()
        {
            AddStep("create room manager with a room", () =>
            {
                var r = createRoom();
                RoomManager.CreateRoom(r);
                RoomManager.PartRoom();
                RoomManager.JoinRoom(r);
            });

            AddUntilStep("multiplayer room joined", () => Client.Room != null);
        }

        private Room createRoom(Action<Room> initFunc = null)
        {
            var room = new Room
            {
                Name =
                {
                    Value = "test room"
                },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = new TestBeatmap(Ruleset.Value).BeatmapInfo },
                        Ruleset = { Value = Ruleset.Value }
                    }
                }
            };

            initFunc?.Invoke(room);
            return room;
        }

        private class TestDependencies : MultiplayerRoomTestDependencies
        {
            public TestDependencies()
            {
                // Need to set these values as early as possible.
                RoomManager.TimeBetweenListingPolls.Value = 1;
                RoomManager.TimeBetweenSelectionPolls.Value = 1;
            }
        }
    }
}
