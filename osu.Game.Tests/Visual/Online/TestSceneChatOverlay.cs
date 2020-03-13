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
using osu.Game.Graphics.UserInterface;
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
        private readonly List<Channel> channels;

        private Channel currentChannel => channelManager.CurrentChannel.Value;
        private Channel nextChannel => joinedChannels.ElementAt(joinedChannels.ToList().IndexOf(currentChannel) + 1);
        private Channel previousChannel => joinedChannels.ElementAt(joinedChannels.ToList().IndexOf(currentChannel) - 1);
        private Channel channel1 => channels[0];
        private Channel channel2 => channels[1];

        public TestSceneChatOverlay()
        {
            channels = Enumerable.Range(1, 10)
                                 .Select(index => new Channel(new User())
                                 {
                                     Name = $"Channel no. {index}",
                                     Topic = index == 3 ? null : $"We talk about the number {index} here"
                                 })
                                 .ToList();
        }

        [SetUp]
        public void Setup()
        {
            Schedule(() =>
            {
                ChannelManagerContainer container;

                Child = container = new ChannelManagerContainer(channels)
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

            AddAssert("Current channel is channel 1", () => currentChannel == channel1);
            AddAssert("Channel selector was closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
        }

        [Test]
        public void TestCloseChannelWhileSelectorClosed()
        {
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(channel2));

            AddStep("Switch to channel 2", () => clickDrawable(chatOverlay.TabMap[channel2]));
            AddStep("Close channel 2", () => clickDrawable(((TestPrivateChannelTabItem)chatOverlay.TabMap[channel2]).CloseButton.Child));

            AddAssert("Selector remained closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Current channel is channel 1", () => currentChannel == channel1);

            AddStep("Close channel 1", () => clickDrawable(((TestPrivateChannelTabItem)chatOverlay.TabMap[channel1]).CloseButton.Child));

            AddAssert("Selector is visible", () => chatOverlay.SelectionOverlayState == Visibility.Visible);
        }

        [Test]
        public void TestSearchInSelector()
        {
            AddStep("search for 'no. 2'", () => chatOverlay.ChildrenOfType<SearchTextBox>().First().Text = "no. 2");
            AddUntilStep("only channel 2 visible", () =>
            {
                var listItems = chatOverlay.ChildrenOfType<ChannelListItem>().Where(c => c.IsPresent);
                return listItems.Count() == 1 && listItems.Single().Channel == channel2;
            });
        }

        [Test]
        public void TestChannelShortcutKeys()
        {
            AddStep("join 10 channels", () => channels.ForEach(channel => channelManager.JoinChannel(channel)));
            AddStep("close channel selector", () =>
            {
                InputManager.PressKey(Key.Escape);
                InputManager.ReleaseKey(Key.Escape);
            });
            AddUntilStep("wait for close", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);

            for (int zeroBasedIndex = 0; zeroBasedIndex < 10; ++zeroBasedIndex)
            {
                var oneBasedIndex = zeroBasedIndex + 1;
                var targetNumberKey = oneBasedIndex % 10;
                var targetChannel = channels[zeroBasedIndex];
                AddStep($"press Alt+{targetNumberKey}", () => pressChannelHotkey(targetNumberKey));
                AddAssert($"channel #{oneBasedIndex} is selected", () => currentChannel == targetChannel);
            }
        }

        private Channel expectedChannel;

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
            AddAssert("Last visible selected", () => currentChannel == visibleChannels.Last());

            // Closing the last channel before dropdown
            AddStep("Close current channel", () =>
            {
                expectedChannel = nextChannel;
                chatOverlay.ChannelTabControl.RemoveChannel(currentChannel);
            });
            AddAssert("Next channel selected", () => currentChannel == expectedChannel);

            // Depending on the window size, one more channel might need to be closed for the selectorTab to appear
            AddUntilStep("Close channels until selector visible", () =>
            {
                if (chatOverlay.ChannelTabControl.VisibleItems.Last().Name == "+")
                    return true;

                chatOverlay.ChannelTabControl.RemoveChannel(visibleChannels.Last());
                return false;
            });
            AddAssert("Last visible selected", () => currentChannel == visibleChannels.Last());

            // Closing the last channel with dropdown no longer present
            AddStep("Close last when selector next", () =>
            {
                expectedChannel = previousChannel;
                chatOverlay.ChannelTabControl.RemoveChannel(currentChannel);
            });
            AddAssert("Channel changed to previous", () => currentChannel == expectedChannel);

            // Standard channel closing
            AddStep("Switch to previous channel", () => chatOverlay.ChannelTabControl.SwitchTab(-1));
            AddStep("Close current channel", () =>
            {
                expectedChannel = nextChannel;
                chatOverlay.ChannelTabControl.RemoveChannel(currentChannel);
            });
            AddAssert("Channel changed to next", () => currentChannel == expectedChannel);
        }

        private void pressChannelHotkey(int number)
        {
            var channelKey = Key.Number0 + number;
            InputManager.PressKey(Key.AltLeft);
            InputManager.PressKey(channelKey);
            InputManager.ReleaseKey(Key.AltLeft);
            InputManager.ReleaseKey(channelKey);
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

                Child = ChatOverlay = new TestChatOverlay { RelativeSizeAxes = Axes.Both, };
                ChatOverlay.Show();
            }
        }

        private class TestChatOverlay : ChatOverlay
        {
            public Visibility SelectionOverlayState => ChannelSelectionOverlay.State.Value;

            public new ChannelTabControl ChannelTabControl => base.ChannelTabControl;

            public new ChannelSelectionOverlay ChannelSelectionOverlay => base.ChannelSelectionOverlay;

            protected override ChannelTabControl CreateChannelTabControl() => new TestTabControl();

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
