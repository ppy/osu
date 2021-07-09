// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerLoungeSubScreen : OnlinePlayTestScene
    {
        protected new BasicTestRoomManager RoomManager => (BasicTestRoomManager)base.RoomManager;

        private LoungeSubScreen loungeScreen;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(loungeScreen = new MultiplayerLoungeSubScreen()));

            AddUntilStep("wait for present", () => loungeScreen.IsCurrentScreen());
        }

        [Test]
        public void TestJoinRoomWithoutPassword()
        {
            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: false));
        }

        [Test]
        public void TestJoinRoomWithPassword()
        {
            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
        }

        private RoomsContainer roomsContainer => loungeScreen.ChildrenOfType<RoomsContainer>().First();
    }
}
