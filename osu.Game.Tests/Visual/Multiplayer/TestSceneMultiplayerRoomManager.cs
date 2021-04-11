// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [HeadlessTest]
    public class TestSceneMultiplayerRoomManager : RoomTestScene
    {
        private TestMultiplayerRoomContainer roomContainer;
        private TestMultiplayerRoomManager roomManager => roomContainer.RoomManager;

        [Test]
        public void TestPollsInitially()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(createRoom(r => r.Name.Value = "1"));
                    roomManager.PartRoom();
                    roomManager.CreateRoom(createRoom(r => r.Name.Value = "2"));
                    roomManager.PartRoom();
                    roomManager.ClearRooms();
                });
            });

            AddAssert("manager polled for rooms", () => ((RoomManager)roomManager).Rooms.Count == 2);
            AddAssert("initial rooms received", () => roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsClearedOnDisconnection()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(createRoom());
                    roomManager.PartRoom();
                    roomManager.CreateRoom(createRoom());
                    roomManager.PartRoom();
                });
            });

            AddStep("disconnect", () => roomContainer.Client.Disconnect());

            AddAssert("rooms cleared", () => ((RoomManager)roomManager).Rooms.Count == 0);
            AddAssert("initial rooms not received", () => !roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsPolledOnReconnect()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(createRoom());
                    roomManager.PartRoom();
                    roomManager.CreateRoom(createRoom());
                    roomManager.PartRoom();
                });
            });

            AddStep("disconnect", () => roomContainer.Client.Disconnect());
            AddStep("connect", () => roomContainer.Client.Connect());

            AddAssert("manager polled for rooms", () => ((RoomManager)roomManager).Rooms.Count == 2);
            AddAssert("initial rooms received", () => roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsNotPolledWhenJoined()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(createRoom());
                    roomManager.ClearRooms();
                });
            });

            AddAssert("manager not polled for rooms", () => ((RoomManager)roomManager).Rooms.Count == 0);
            AddAssert("initial rooms not received", () => !roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestMultiplayerRoomJoinedWhenCreated()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(createRoom());
                });
            });

            AddUntilStep("multiplayer room joined", () => roomContainer.Client.Room != null);
        }

        [Test]
        public void TestMultiplayerRoomPartedWhenAPIRoomParted()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(createRoom());
                    roomManager.PartRoom();
                });
            });

            AddAssert("multiplayer room parted", () => roomContainer.Client.Room == null);
        }

        [Test]
        public void TestMultiplayerRoomJoinedWhenAPIRoomJoined()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    var r = createRoom();
                    roomManager.CreateRoom(r);
                    roomManager.PartRoom();
                    roomManager.JoinRoom(r);
                });
            });

            AddUntilStep("multiplayer room joined", () => roomContainer.Client.Room != null);
        }

        private Room createRoom(Action<Room> initFunc = null)
        {
            var room = new Room();

            room.Name.Value = "test room";
            room.Playlist.Add(new PlaylistItem
            {
                Beatmap = { Value = new TestBeatmap(Ruleset.Value).BeatmapInfo },
                Ruleset = { Value = Ruleset.Value }
            });

            initFunc?.Invoke(room);
            return room;
        }

        private TestMultiplayerRoomManager createRoomManager()
        {
            Child = roomContainer = new TestMultiplayerRoomContainer
            {
                RoomManager =
                {
                    TimeBetweenListingPolls = { Value = 1 },
                    TimeBetweenSelectionPolls = { Value = 1 }
                }
            };

            return roomManager;
        }
    }
}
