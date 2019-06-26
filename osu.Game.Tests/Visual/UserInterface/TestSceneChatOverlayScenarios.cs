// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat.Tabs;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneChatOverlayScenarios : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChannelTabControl),
            typeof(ChannelTabItem),
            typeof(ChatOverlay),
        };

        private TestChatOverlay chatOverlay;

        [Cached]
        private ChannelManager channelManager = new ChannelManager();

        private Channel channel1;
        private Channel channel2;

        [BackgroundDependencyLoader]
        private void load()
        {
            var availableChannels = (BindableList<Channel>)channelManager.AvailableChannels;

            availableChannels.Add(channel1 = new Channel(new User()) { Name = "test1" });
            availableChannels.Add(channel2 = new Channel(new User()) { Name = "test2" });

            Add(chatOverlay = new TestChatOverlay
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(1)
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Leave channels", () =>
            {
                channelManager.LeaveChannel(channel1);
                channelManager.LeaveChannel(channel2);
            });
            AddStep("Show chat", () => chatOverlay.Show());
        }

        /// <summary>
        /// Test that if no maps are added, the channel selector is also toggled when <see cref="ChatOverlay"/> is toggled.
        /// Also check that both are properly closed when toggling again.
        /// </summary>
        [Test]
        public void TestToggleChatWithNoChannelsJoined()
        {
            AddAssert("Channel selection overlay was toggled", () => chatOverlay.SelectionOverlayState == Visibility.Visible);
            AddAssert("Chat overlay was shown", () => chatOverlay.State.Value == Visibility.Visible);
            AddStep("Close chat overlay", () => chatOverlay.Hide());
            AddAssert("Channel selection overlay was hidden", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Chat overlay was hidden", () => chatOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestToggleChatWithChannelJoined()
        {
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Close chat overlay", () => chatOverlay.Hide());
            AddAssert("Channel selection overlay was hidden", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Chat overlay was hidden", () => chatOverlay.State.Value == Visibility.Hidden);
            AddStep("Close chat overlay", () => chatOverlay.Show());
            AddAssert("Channel selection overlay was not toggled", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Chat overlay was shown", () => chatOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestTabbingAwayClosesSelector()
        {
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(channel2));
            AddStep("Switch to channel 2", () => clickDrawable(chatOverlay.TabMap[channel2]));
            AddAssert("Current channel is channel 2", () => channelManager.CurrentChannel.Value == channel2);
            AddAssert("Channel selector was closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
        }

        [Test]
        public void TestCloseChannelWhileSelectorClosed()
        {
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(channel2));
            AddStep("Switch to channel 2", () => clickDrawable(chatOverlay.TabMap[channel2]));
            AddStep("Close channel 2", () => clickDrawable(((TestChannelTabItem)chatOverlay.TabMap[channel2]).CloseButton.Child));
            AddAssert("Selector remained closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Current channel is channel 2", () => channelManager.CurrentChannel.Value == channel1);
            AddStep("Close channel 1", () => clickDrawable(((TestChannelTabItem)chatOverlay.TabMap[channel1]).CloseButton.Child));
            AddAssert("Channel selection overlay was toggled", () => chatOverlay.SelectionOverlayState == Visibility.Visible);
        }

        private void clickDrawable(Drawable d)
        {
            InputManager.MoveMouseTo(d);
            InputManager.Click(MouseButton.Left);
        }

        private class TestChatOverlay : ChatOverlay
        {
            public Visibility SelectionOverlayState => ChannelSelectionOverlay.State.Value;

            protected override ChannelTabControl CreateChannelTabControl() => new TestTabControl();

            public IReadOnlyDictionary<Channel, TabItem<Channel>> TabMap => ((TestTabControl)ChannelTabControl).TabMap;
        }

        private class TestTabControl : ChannelTabControl
        {
            protected override TabItem<Channel> CreateTabItem(Channel value) => new TestChannelTabItem(value) { OnRequestClose = TabCloseRequested };

            public new IReadOnlyDictionary<Channel, TabItem<Channel>> TabMap => base.TabMap;
        }

        private class TestChannelTabItem : PrivateChannelTabItem
        {
            public TestChannelTabItem(Channel channel)
                : base(channel)
            {
            }

            public new ClickableContainer CloseButton => base.CloseButton;
        }
    }
}
