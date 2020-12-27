// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;

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
                    roomManager.CreateRoom(new Room { Name = { Value = "1" } });
                    roomManager.PartRoom();
                    roomManager.CreateRoom(new Room { Name = { Value = "2" } });
                    roomManager.PartRoom();
                    roomManager.ClearRooms();
                });
            });

            AddAssert("manager polled for rooms", () => roomManager.Rooms.Count == 2);
            AddAssert("initial rooms received", () => roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsClearedOnDisconnection()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(new Room());
                    roomManager.PartRoom();
                    roomManager.CreateRoom(new Room());
                    roomManager.PartRoom();
                });
            });

            AddStep("disconnect", () => roomContainer.Client.Disconnect());

            AddAssert("rooms cleared", () => roomManager.Rooms.Count == 0);
            AddAssert("initial rooms not received", () => !roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsPolledOnReconnect()
        {
            AddStep("create room manager with a few rooms", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(new Room());
                    roomManager.PartRoom();
                    roomManager.CreateRoom(new Room());
                    roomManager.PartRoom();
                });
            });

            AddStep("disconnect", () => roomContainer.Client.Disconnect());
            AddStep("connect", () => roomContainer.Client.Connect());

            AddAssert("manager polled for rooms", () => roomManager.Rooms.Count == 2);
            AddAssert("initial rooms received", () => roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestRoomsNotPolledWhenJoined()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(new Room());
                    roomManager.ClearRooms();
                });
            });

            AddAssert("manager not polled for rooms", () => roomManager.Rooms.Count == 0);
            AddAssert("initial rooms not received", () => !roomManager.InitialRoomsReceived.Value);
        }

        [Test]
        public void TestMultiplayerRoomJoinedWhenCreated()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(new Room());
                });
            });

            AddAssert("multiplayer room joined", () => roomContainer.Client.Room != null);
        }

        [Test]
        public void TestMultiplayerRoomPartedWhenAPIRoomParted()
        {
            AddStep("create room manager with a room", () =>
            {
                createRoomManager().With(d => d.OnLoadComplete += _ =>
                {
                    roomManager.CreateRoom(new Room());
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
                    var r = new Room();
                    roomManager.CreateRoom(r);
                    roomManager.PartRoom();
                    roomManager.JoinRoom(r);
                });
            });

            AddAssert("multiplayer room joined", () => roomContainer.Client.Room != null);
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
