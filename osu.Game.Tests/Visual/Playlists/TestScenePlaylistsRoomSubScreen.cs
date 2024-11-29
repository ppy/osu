// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsRoomSubScreen : OnlinePlayTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        protected new TestRoomManager RoomManager => (TestRoomManager)base.RoomManager;

        [Test]
        public void TestStatusUpdateOnEnter()
        {
            Room room = null!;
            PlaylistsRoomSubScreen roomScreen = null!;

            AddStep("create room", () =>
            {
                RoomManager.AddRoom(room = new Room
                {
                    Name = @"Test Room",
                    Host = new APIUser { Username = @"Host" },
                    Category = RoomCategory.Normal,
                    EndDate = DateTimeOffset.Now.AddMinutes(-1)
                });
            });

            AddStep("push screen", () => LoadScreen(roomScreen = new PlaylistsRoomSubScreen(room)));
            AddUntilStep("wait for screen load", () => roomScreen.IsCurrentScreen());
            AddAssert("status is still ended", () => roomScreen.Room.Status, Is.TypeOf<RoomStatusEnded>);
        }

        [Test]
        public void TestCloseButtonGoesAwayAfterGracePeriod()
        {
            Room room = null!;
            PlaylistsRoomSubScreen roomScreen = null!;

            AddStep("create room", () =>
            {
                RoomManager.AddRoom(room = new Room
                {
                    Name = @"Test Room",
                    Host = api.LocalUser.Value,
                    Category = RoomCategory.Normal,
                    StartDate = DateTimeOffset.Now.AddMinutes(-5).AddSeconds(3),
                    EndDate = DateTimeOffset.Now.AddMinutes(30)
                });
            });

            AddStep("push screen", () => LoadScreen(roomScreen = new PlaylistsRoomSubScreen(room)));
            AddUntilStep("wait for screen load", () => roomScreen.IsCurrentScreen());
            AddAssert("close button present", () => roomScreen.ChildrenOfType<DangerousRoundedButton>().Any());
            AddUntilStep("wait for close button to disappear", () => !roomScreen.ChildrenOfType<DangerousRoundedButton>().Any());
        }
    }
}
