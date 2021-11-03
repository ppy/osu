// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneGameplayChatDisplay : MultiplayerTestScene
    {
        private GameplayChatDisplay chatDisplay;

        [Cached(typeof(ILocalUserPlayInfo))]
        private ILocalUserPlayInfo localUserInfo;

        private readonly Bindable<bool> localUserPlaying = new Bindable<bool>();

        private TextBox textBox => chatDisplay.ChildrenOfType<TextBox>().First();

        public TestSceneGameplayChatDisplay()
        {
            var mockLocalUserInfo = new Mock<ILocalUserPlayInfo>();
            mockLocalUserInfo.SetupGet(i => i.IsPlaying).Returns(localUserPlaying);

            localUserInfo = mockLocalUserInfo.Object;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load chat display", () => Child = chatDisplay = new GameplayChatDisplay(SelectedRoom.Value)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.5f,
            });

            AddStep("expand", () => chatDisplay.Expanded.Value = true);
        }

        [Test]
        public void TestCantClickWhenPlaying()
        {
            setLocalUserPlaying(true);

            AddStep("attempt focus chat", () =>
            {
                InputManager.MoveMouseTo(textBox);
                InputManager.Click(MouseButton.Left);
            });

            assertChatFocused(false);
        }

        [Test]
        public void TestFocusDroppedWhenPlaying()
        {
            assertChatFocused(false);

            AddStep("focus chat", () =>
            {
                InputManager.MoveMouseTo(textBox);
                InputManager.Click(MouseButton.Left);
            });

            setLocalUserPlaying(true);
            assertChatFocused(false);

            // should still stay non-focused even after entering a new break section.
            setLocalUserPlaying(false);
            assertChatFocused(false);
        }

        [Test]
        public void TestFocusOnTabKeyWhenExpanded()
        {
            setLocalUserPlaying(true);

            assertChatFocused(false);
            AddStep("press tab", () => InputManager.Key(Key.Tab));
            assertChatFocused(true);
        }

        [Test]
        public void TestFocusLostOnBackKey()
        {
            setLocalUserPlaying(true);

            assertChatFocused(false);
            AddStep("press tab", () => InputManager.Key(Key.Tab));
            assertChatFocused(true);
            AddStep("press escape", () => InputManager.Key(Key.Escape));
            assertChatFocused(false);
        }

        [Test]
        public void TestFocusOnTabKeyWhenNotExpanded()
        {
            AddStep("set not expanded", () => chatDisplay.Expanded.Value = false);
            AddUntilStep("is not visible", () => !chatDisplay.IsPresent);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            assertChatFocused(true);
            AddUntilStep("is visible", () => chatDisplay.IsPresent);

            AddStep("press enter", () => InputManager.Key(Key.Enter));
            assertChatFocused(false);
            AddUntilStep("is not visible", () => !chatDisplay.IsPresent);
        }

        [Test]
        public void TestFocusToggleViaAction()
        {
            AddStep("set not expanded", () => chatDisplay.Expanded.Value = false);
            AddUntilStep("is not visible", () => !chatDisplay.IsPresent);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            assertChatFocused(true);
            AddUntilStep("is visible", () => chatDisplay.IsPresent);

            AddStep("press tab", () => InputManager.Key(Key.Tab));
            assertChatFocused(false);
            AddUntilStep("is not visible", () => !chatDisplay.IsPresent);
        }

        private void assertChatFocused(bool isFocused) =>
            AddAssert($"chat {(isFocused ? "focused" : "not focused")}", () => textBox.HasFocus == isFocused);

        private void setLocalUserPlaying(bool playing) =>
            AddStep($"local user {(playing ? "playing" : "not playing")}", () => localUserPlaying.Value = playing);
    }
}
