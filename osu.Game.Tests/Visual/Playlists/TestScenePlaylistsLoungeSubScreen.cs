// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Playlists
{
    public class TestScenePlaylistsLoungeSubScreen : RoomManagerTestScene
    {
        private LoungeSubScreen loungeScreen;

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(loungeScreen = new PlaylistsLoungeSubScreen
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
            }));

            AddUntilStep("wait for present", () => loungeScreen.IsCurrentScreen());
        }

        private RoomsContainer roomsContainer => loungeScreen.ChildrenOfType<RoomsContainer>().First();

        [Test]
        public void TestScrollSelectedIntoView()
        {
            AddRooms(30);

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
