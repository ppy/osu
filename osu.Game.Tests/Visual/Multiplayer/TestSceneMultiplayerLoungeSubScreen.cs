// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
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
        protected new TestRoomManager RoomManager => (TestRoomManager)base.RoomManager;

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
        public void TestPopoverHidesOnBackButton()
        {
            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("attempt join room", () => InputManager.Key(Key.Enter));

            AddUntilStep("password prompt appeared", () => InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().Any());

            AddAssert("textbox has focus", () => InputManager.FocusedDrawable is OsuPasswordTextBox);

            AddStep("hit escape", () => InputManager.Key(Key.Escape));
            AddAssert("textbox lost focus", () => InputManager.FocusedDrawable is SearchTextBox);

            AddStep("hit escape", () => InputManager.Key(Key.Escape));
            AddUntilStep("password prompt hidden", () => !InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().Any());
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
        public void TestJoinRoomWithIncorrectPasswordViaButton()
        {
            DrawableLoungeRoom.PasswordEntryPopover passwordEntryPopover = null;

            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("attempt join room", () => InputManager.Key(Key.Enter));
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().FirstOrDefault()) != null);
            AddStep("enter password in text box", () => passwordEntryPopover.ChildrenOfType<TextBox>().First().Text = "wrong");
            AddStep("press join room button", () => passwordEntryPopover.ChildrenOfType<OsuButton>().First().TriggerClick());

            AddAssert("room not joined", () => loungeScreen.IsCurrentScreen());
            AddUntilStep("password prompt still visible", () => passwordEntryPopover.State.Value == Visibility.Visible);
            AddAssert("textbox still focused", () => InputManager.FocusedDrawable is OsuPasswordTextBox);
        }

        [Test]
        public void TestJoinRoomWithIncorrectPasswordViaEnter()
        {
            DrawableLoungeRoom.PasswordEntryPopover passwordEntryPopover = null;

            AddStep("add room", () => RoomManager.AddRooms(1, withPassword: true));
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("attempt join room", () => InputManager.Key(Key.Enter));
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().FirstOrDefault()) != null);
            AddStep("enter password in text box", () => passwordEntryPopover.ChildrenOfType<TextBox>().First().Text = "wrong");
            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddAssert("room not joined", () => loungeScreen.IsCurrentScreen());
            AddUntilStep("password prompt still visible", () => passwordEntryPopover.State.Value == Visibility.Visible);
            AddAssert("textbox still focused", () => InputManager.FocusedDrawable is OsuPasswordTextBox);
        }

        [Test]
        public void TestJoinRoomWithCorrectPassword()
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
