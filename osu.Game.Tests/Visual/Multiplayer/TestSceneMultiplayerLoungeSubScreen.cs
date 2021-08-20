// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Tests.Visual.OnlinePlay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerLoungeSubScreen : OnlinePlayTestScene
    {
        protected new TestRequestHandlingRoomManager RoomManager => (TestRequestHandlingRoomManager)base.RoomManager;

        private LoungeSubScreen loungeScreen;

        private Room lastJoinedRoom;
        private string lastJoinedPassword;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("push screen", () => LoadScreen(loungeScreen = new MultiplayerLoungeSubScreen()));

            AddUntilStep("wait for present", () => loungeScreen.IsCurrentScreen());

            AddStep("bind to event", () =>
            {
                lastJoinedRoom = null;
                lastJoinedPassword = null;
                RoomManager.JoinRoomRequested = onRoomJoined;
            });
        }

        [Test]
        public void TestJoinRoomWithoutPassword()
        {
            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: false));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            AddAssert("room join requested", () => lastJoinedRoom == RoomManager.Rooms.First());
            AddAssert("room join password correct", () => lastJoinedPassword == null);
        }

        [Test]
        public void TestPopoverHidesOnLeavingScreen()
        {
            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("attempt join room", () => InputManager.Key(Key.Enter));

            AddUntilStep("password prompt appeared", () => InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().Any());
            AddStep("exit screen", () => Stack.Exit());
            AddUntilStep("password prompt hidden", () => !InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().Any());
        }

        [Test]
        public void TestJoinRoomWithPassword()
        {
            DrawableLoungeRoom.PasswordEntryPopover passwordEntryPopover = null;

            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("attempt join room", () => InputManager.Key(Key.Enter));
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().FirstOrDefault()) != null);
            AddStep("enter password in text box", () => passwordEntryPopover.ChildrenOfType<TextBox>().First().Text = "password");
            AddStep("press join room button", () => passwordEntryPopover.ChildrenOfType<OsuButton>().First().TriggerClick());

            AddAssert("room join requested", () => lastJoinedRoom == RoomManager.Rooms.First());
            AddAssert("room join password correct", () => lastJoinedPassword == "password");
        }

        [Test]
        public void TestJoinRoomWithPasswordViaKeyboardOnly()
        {
            DrawableLoungeRoom.PasswordEntryPopover passwordEntryPopover = null;

            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("attempt join room", () => InputManager.Key(Key.Enter));
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().FirstOrDefault()) != null);
            AddStep("enter password in text box", () => passwordEntryPopover.ChildrenOfType<TextBox>().First().Text = "password");
            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddAssert("room join requested", () => lastJoinedRoom == RoomManager.Rooms.First());
            AddAssert("room join password correct", () => lastJoinedPassword == "password");
        }

        private void onRoomJoined(Room room, string password)
        {
            lastJoinedRoom = room;
            lastJoinedPassword = password;
        }
    }
}
