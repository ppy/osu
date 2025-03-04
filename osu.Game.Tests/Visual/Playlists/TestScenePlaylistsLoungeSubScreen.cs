// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistsLoungeSubScreen : OnlinePlayTestScene
    {
        private PlaylistsLoungeSubScreen loungeScreen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(loungeScreen = new PlaylistsLoungeSubScreen()));
            AddUntilStep("wait for present", () => loungeScreen.IsCurrentScreen());
        }

        private RoomListing roomListing => loungeScreen.ChildrenOfType<RoomListing>().First();

        [Test]
        public void TestManyRooms()
        {
            createRooms(GenerateRooms(500));
        }

        [Test]
        public void TestScrollByDraggingRooms()
        {
            AddStep("reset mouse", () => InputManager.ReleaseButton(MouseButton.Left));

            createRooms(GenerateRooms(30));

            AddStep("move mouse to third room", () => InputManager.MoveMouseTo(roomListing.DrawableRooms[2]));
            AddStep("hold down", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag to top", () => InputManager.MoveMouseTo(roomListing.DrawableRooms[0]));

            AddAssert("first and second room masked", ()
                => !checkRoomVisible(roomListing.DrawableRooms[0]) &&
                   !checkRoomVisible(roomListing.DrawableRooms[1]));
        }

        [Test]
        public void TestScrollSelectedIntoView()
        {
            createRooms(GenerateRooms(30));

            AddStep("select last room", () => roomListing.DrawableRooms[^1].TriggerClick());

            AddUntilStep("first room is masked", () => !checkRoomVisible(roomListing.DrawableRooms[0]));
            AddUntilStep("last room is not masked", () => checkRoomVisible(roomListing.DrawableRooms[^1]));
        }

        private bool checkRoomVisible(DrawableRoom room) =>
            loungeScreen.ChildrenOfType<OsuScrollContainer>().First().ScreenSpaceDrawQuad
                        .Contains(room.ScreenSpaceDrawQuad.Centre);

        private void createRooms(params Room[] rooms)
        {
            AddStep("create rooms", () =>
            {
                foreach (var room in rooms)
                    API.Queue(new CreateRoomRequest(room));
            });

            AddStep("refresh lounge", () => loungeScreen.RefreshRooms());
        }
    }
}
