// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsRoomSubScreen : OnlinePlayTestScene
    {
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
    }
}
