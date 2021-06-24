// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Visual.OnlinePlay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneLoungeRoomInfo : OsuTestScene
    {
        private TestRoomContainer roomContainer;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = roomContainer = new TestRoomContainer
            {
                Child = new RoomInfo
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500
                }
            };
        });

        [Test]
        public void TestNonSelectedRoom()
        {
            AddStep("set null room", () => roomContainer.Room.RoomID.Value = null);
        }

        [Test]
        public void TestOpenRoom()
        {
            AddStep("set open room", () =>
            {
                roomContainer.Room.RoomID.Value = 0;
                roomContainer.Room.Name.Value = "Room 0";
                roomContainer.Room.Host.Value = new User { Username = "peppy", Id = 2 };
                roomContainer.Room.EndDate.Value = DateTimeOffset.Now.AddMonths(1);
                roomContainer.Room.Status.Value = new RoomStatusOpen();
            });
        }
    }
}
