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
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.Selection;
using osu.Game.Overlays.Chat.Tabs;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneChatOverlay : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatLine),
            typeof(DrawableChannel),
            typeof(ChannelSelectorTabItem),
            typeof(ChannelTabControl),
            typeof(ChannelTabItem),
            typeof(PrivateChannelTabItem),
            typeof(TabCloseButton)
        };

        private TestChatOverlay chatOverlay;
        private ChannelManager channelManager;

        private IEnumerable<Channel> visibleChannels => chatOverlay.ChannelTabControl.VisibleItems.Where(channel => channel.Name != "+");
        private IEnumerable<Channel> joinedChannels => chatOverlay.ChannelTabControl.Items.Where(channel => channel.Name != "+");

        private readonly Channel channel1 = new Channel(new User()) { Name = "test really long username" };
        private readonly Channel channel2 = new Channel(new User()) { Name = "test2" };

        [SetUp]
        public void Setup()
        {
            Schedule(() =>
            {
                ChannelManagerContainer container;

                Child = container = new ChannelManagerContainer(new List<Channel> { channel1, channel2 })
                {
                    RelativeSizeAxes = Axes.Both,
                };

                chatOverlay = container.ChatOverlay;
                channelManager = container.ChannelManager;
            });
        }

        [Test]
        public void TestHideOverlay()
        {
            AddAssert("Chat overlay is visible", () => chatOverlay.State.Value == Visibility.Visible);
            AddAssert("Selector is visible", () => chatOverlay.SelectionOverlayState == Visibility.Visible);

            AddStep("Close chat overlay", () => chatOverlay.Hide());

            AddAssert("Chat overlay was hidden", () => chatOverlay.State.Value == Visibility.Hidden);
            AddAssert("Channel selection overlay was hidden", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
        }

        [Test]
        public void TestSelectingChannelClosesSelector()
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
            AddStep("Close channel 2", () => chatOverlay.ChannelTabControl.RemoveChannel(channel2));

            AddAssert("Selector remained closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Current channel is channel 1", () => channelManager.CurrentChannel.Value == channel1);

            AddStep("Close channel 1", () => chatOverlay.ChannelTabControl.RemoveChannel(channel1));

            AddAssert("Selector is visible", () => chatOverlay.SelectionOverlayState == Visibility.Visible);
        }

        private Channel nextChannel;

        [Test]
        public void TestCloseChannelWhileActive()
        {
            AddUntilStep("Join until dropdown has channels", () =>
            {
                if (visibleChannels.Count() < joinedChannels.Count())
                    return true;

                // Using temporary channels because they don't hide their names when not active
                Channel toAdd = new Channel { Name = $"test channel {joinedChannels.Count()}", Type = ChannelType.Temporary };

                channelManager.JoinChannel(toAdd);

                return false;
            });

            AddStep("Switch to last tab", () => clickDrawable(chatOverlay.TabMap[visibleChannels.Last()]));
            AddAssert("Channel is last visible", () => channelManager.CurrentChannel.Value == visibleChannels.Last());

            // Closing the last channel before dropdown
            AddStep("Close current channel", () =>
            {
                nextChannel = joinedChannels.Except(visibleChannels).First();
                chatOverlay.ChannelTabControl.RemoveChannel(channelManager.CurrentChannel.Value);
            });
            AddAssert("Channel changed to next", () => channelManager.CurrentChannel.Value == nextChannel);

            // Depending on the window size, one more channel needs to be closed for SelectorTab to appear
            AddUntilStep("Close channels until selector visible", () =>
            {
                if (chatOverlay.ChannelTabControl.VisibleItems.Last().Name == "+")
                    return true;

                chatOverlay.ChannelTabControl.RemoveChannel(visibleChannels.Last());
                return false;
            });

            // Closing the last channel with dropdown no longer present
            AddStep("Close last when selector next", () => chatOverlay.ChannelTabControl.RemoveChannel(visibleChannels.Last()));
            AddAssert("Channel changed to previous", () => channelManager.CurrentChannel.Value == visibleChannels.Last());

            // Standard channel closing
            AddStep("Switch to previous channel", () => chatOverlay.ChannelTabControl.SwitchTab(-1));
            AddStep("Close current channel", () => chatOverlay.ChannelTabControl.RemoveChannel(channelManager.CurrentChannel.Value));
            AddAssert("Channel changed to next", () => channelManager.CurrentChannel.Value == visibleChannels.Last());
        }

        private void clickDrawable(Drawable d)
        {
            InputManager.MoveMouseTo(d);
            InputManager.Click(MouseButton.Left);
        }

        private class ChannelManagerContainer : Container
        {
            public TestChatOverlay ChatOverlay { get; private set; }

            [Cached]
            public ChannelManager ChannelManager { get; } = new ChannelManager();

            private readonly List<Channel> channels;

            public ChannelManagerContainer(List<Channel> channels)
            {
                this.channels = channels;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                ((BindableList<Channel>)ChannelManager.AvailableChannels).AddRange(channels);

                Child = ChatOverlay = new TestChatOverlay { RelativeSizeAxes = Axes.Both };
                ChatOverlay.Show();
            }
        }

        private class TestChatOverlay : ChatOverlay
        {
            public Visibility SelectionOverlayState => ChannelSelectionOverlay.State.Value;

            public new ChannelSelectionOverlay ChannelSelectionOverlay => base.ChannelSelectionOverlay;

            protected override ChannelTabControl CreateChannelTabControl() => new TestTabControl();

            public new ChannelTabControl ChannelTabControl => base.ChannelTabControl;

            public IReadOnlyDictionary<Channel, TabItem<Channel>> TabMap => ((TestTabControl)ChannelTabControl).TabMap;
        }

        private class TestTabControl : ChannelTabControl
        {
            protected override TabItem<Channel> CreateTabItem(Channel value)
            {
                switch (value.Type)
                {
                    case ChannelType.PM:
                        return new TestPrivateChannelTabItem(value);

                    default:
                        return new TestChannelTabItem(value);
                }
            }

            public new IReadOnlyDictionary<Channel, TabItem<Channel>> TabMap => base.TabMap;
        }

        private class TestChannelTabItem : ChannelTabItem
        {
            public TestChannelTabItem(Channel channel)
                : base(channel)
            {
            }

            public new ClickableContainer CloseButton => base.CloseButton;
        }

        private class TestPrivateChannelTabItem : PrivateChannelTabItem
        {
            public TestPrivateChannelTabItem(Channel channel)
                : base(channel)
            {
            }

            public new ClickableContainer CloseButton => base.CloseButton;
        }
    }
}
