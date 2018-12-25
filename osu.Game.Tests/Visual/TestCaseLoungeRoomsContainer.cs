// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Lounge.Components;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLoungeRoomsContainer : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RoomsContainer),
            typeof(DrawableRoom)
        };

        [Cached(Type = typeof(IRoomManager))]
        private TestRoomManager roomManager = new TestRoomManager();

        public TestCaseLoungeRoomsContainer()
        {
            RoomsContainer rooms;

            Child = rooms = new RoomsContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
                JoinRequested = joinRequested
            };

            int roomId = 0;

            AddStep("Add room", () => roomManager.Rooms.Add(new Room
            {
                Name = { Value = $"Room {++roomId}"},
                Host = { Value = new User { Username = "Host" } },
                EndDate = { Value = DateTimeOffset.Now + TimeSpan.FromSeconds(10) }
            }));

            AddStep("Remove selected", () =>
            {
                if (rooms.SelectedRoom.Value != null)
                    roomManager.Rooms.Remove(rooms.SelectedRoom.Value);
            });
        }

        private void joinRequested(Room room) => room.Status.Value = new JoinedRoomStatus();

        private class TestRoomManager : IRoomManager
        {
            public event Action<Room> OpenRequested;

            public readonly BindableCollection<Room> Rooms = new BindableCollection<Room>();
            IBindableCollection<Room> IRoomManager.Rooms => Rooms;

            public void CreateRoom(Room room) => Rooms.Add(room);

            public void JoinRoom(Room room) => OpenRequested?.Invoke(room);

            public void PartRoom()
            {
            }

            public void Filter(FilterCriteria criteria)
            {
            }
        }

        private class JoinedRoomStatus : RoomStatus
        {
            public override string Message => "Joined";

            public override Color4 GetAppropriateColour(OsuColour colours) => colours.Yellow;
        }
    }
}
