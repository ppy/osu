// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneLoungeRoomInfo : RoomTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new RoomInfo
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 500
            };
        });

        public override void SetUpSteps()
        {
            // Todo: Temp
        }

        [Test]
        public void TestNonSelectedRoom()
        {
            AddStep("set null room", () => Room.RoomID.Value = null);
        }

        [Test]
        public void TestOpenRoom()
        {
            AddStep("set open room", () =>
            {
                Room.RoomID.Value = 0;
                Room.Name.Value = "Room 0";
                Room.Host.Value = new User { Username = "peppy", Id = 2 };
                Room.EndDate.Value = DateTimeOffset.Now.AddMonths(1);
                Room.Status.Value = new RoomStatusOpen();
            });
        }
    }
}
