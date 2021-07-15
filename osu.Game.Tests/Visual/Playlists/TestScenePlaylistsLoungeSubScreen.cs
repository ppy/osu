// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsLoungeSubScreen : OnlinePlayTestScene
    {
        protected new BasicTestRoomManager RoomManager => (BasicTestRoomManager)base.RoomManager;

        private LoungeSubScreen loungeScreen;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(loungeScreen = new PlaylistsLoungeSubScreen()));

            AddUntilStep("wait for present", () => loungeScreen.IsCurrentScreen());
        }

        private RoomsContainer roomsContainer => loungeScreen.ChildrenOfType<RoomsContainer>().First();

        [Test]
        public void TestScrollSelectedIntoView()
        {
            AddStep("add rooms", () => RoomManager.AddRooms(30));

            AddUntilStep("first room is not masked", () => checkRoomVisible(roomsContainer.Rooms.First()));

            AddStep("select last room", () => roomsContainer.Rooms.Last().Action?.Invoke());

            AddUntilStep("first room is masked", () => !checkRoomVisible(roomsContainer.Rooms.First()));
            AddUntilStep("last room is not masked", () => checkRoomVisible(roomsContainer.Rooms.Last()));
        }

        private bool checkRoomVisible(DrawableRoom room) =>
            loungeScreen.ChildrenOfType<OsuScrollContainer>().First().ScreenSpaceDrawQuad
                        .Contains(room.ScreenSpaceDrawQuad.Centre);
    }
}
