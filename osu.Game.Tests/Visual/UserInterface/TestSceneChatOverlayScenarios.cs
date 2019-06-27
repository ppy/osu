// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat.Selection;
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
        private ChannelManager channelManager;

        private readonly Channel channel1 = new Channel(new User()) { Name = "test1" };
        private readonly Channel channel2 = new Channel(new User()) { Name = "test2" };

        [SetUp]
        public void Setup()
        {
            Schedule(() =>
            {
                ChannelManagerContainer container;
                Child = container = new ChannelManagerContainer(new List<Channel> { channel1, channel2 }) { RelativeSizeAxes = Axes.Both, };
                chatOverlay = container.ChatOverlay;
                channelManager = container.ChannelManager;
                chatOverlay.Show();
            });
        }

        [Test]
        public void TestHideOverlay()
        {
            AddStep("Close chat overlay", () => chatOverlay.Hide());
            AddAssert("Channel selection overlay was hidden", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Chat overlay was hidden", () => chatOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestTabbingAwayClosesSelector()
        {
            AddAssert("Selector is visible", () => chatOverlay.SelectionOverlayState == Visibility.Visible);
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Switch to channel 1", () => clickDrawable(chatOverlay.TabMap[channel1]));
            AddAssert("Current channel is channel 1", () => channelManager.CurrentChannel.Value == channel1);
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
            AddAssert("Current channel is channel 1", () => channelManager.CurrentChannel.Value == channel1);
            AddStep("Close channel 1", () => clickDrawable(((TestChannelTabItem)chatOverlay.TabMap[channel1]).CloseButton.Child));
            AddAssert("Selector is visible", () => chatOverlay.SelectionOverlayState == Visibility.Visible);
        }

        private void clickDrawable(Drawable d)
        {
            InputManager.MoveMouseTo(d);
            InputManager.Click(MouseButton.Left);
        }

        private class ChannelManagerContainer : Container
        {
            [Cached]
            private ChannelManager channelManager = new ChannelManager();

            public TestChatOverlay ChatOverlay { get; private set; }

            public ChannelManager ChannelManager => channelManager;

            private readonly List<Channel> channels;

            public ChannelManagerContainer(List<Channel> channels)
            {
                this.channels = channels;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                var availableChannels = (BindableList<Channel>)channelManager.AvailableChannels;

                availableChannels.AddRange(channels);

                Child = ChatOverlay = new TestChatOverlay
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1)
                };
            }
        }

        private class TestChatOverlay : ChatOverlay
        {
            public Visibility SelectionOverlayState => ChannelSelectionOverlay.State.Value;

            public new ChannelSelectionOverlay ChannelSelectionOverlay => base.ChannelSelectionOverlay;

            protected override ChannelTabControl CreateChannelTabControl() => new TestTabControl();

            public IReadOnlyDictionary<Channel, TabItem<Channel>> TabMap => ((TestTabControl)ChannelTabControl).TabMap;
        }

        private class TestTabControl : ChannelTabControl
        {
            protected override TabItem<Channel> CreateTabItem(Channel value) => new TestChannelTabItem(value);

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
