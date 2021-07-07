// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneLoungeRoomInfo : OnlinePlayTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room();

            Child = new RoomInfo
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 500
            };
        });

        [Test]
        public void TestNonSelectedRoom()
        {
            AddStep("set null room", () => SelectedRoom.Value.RoomID.Value = null);
        }

        [Test]
        public void TestOpenRoom()
        {
            AddStep("set open room", () =>
            {
                SelectedRoom.Value.RoomID.Value = 0;
                SelectedRoom.Value.Name.Value = "Room 0";
                SelectedRoom.Value.Host.Value = new User { Username = "peppy", Id = 2 };
                SelectedRoom.Value.EndDate.Value = DateTimeOffset.Now.AddMonths(1);
                SelectedRoom.Value.Status.Value = new RoomStatusOpen();
            });
        }
    }
}
