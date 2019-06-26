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
            AddStep("Hide chat", () => chatOverlay.Hide());
        }

        /// <summary>
        /// Test that if no maps are added, the channel selector is also toggled when <see cref="ChatOverlay"/> is toggled.
        /// Also check that both are properly closed when toggling again.
        /// </summary>
        [Test]
        public void TestToggleChatWithNoChannelsJoined()
        {
            AddStep("Toggle chat overlay", () => chatOverlay.Show());
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
            AddStep("Toggle chat overlay", () => chatOverlay.Show());
            AddAssert("Channel selection overlay was not toggled", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Chat overlay was shown", () => chatOverlay.State.Value == Visibility.Visible);
            AddStep("Close chat overlay", () => chatOverlay.Hide());
            AddAssert("Channel selection overlay was hidden", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Chat overlay was hidden", () => chatOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestTabbingAwayClosesSelector()
        {
            AddStep("Toggle chat overlay", () => chatOverlay.Show());
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(channel2));

            // There is currently no way to map a tab drawable to its respective value at this level, so this test relies on the tab's location in AvailableTabs
            AddStep("Switch to channel 2", () => clickDrawable(chatOverlay.AvailableTabs.First()));
            AddAssert("Current channel is channel 2", () => channelManager.CurrentChannel.Value == channel2);
            AddAssert("Channel selector was closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
        }

        [Test]
        public void TestCloseChannelWhileSelectorClosed()
        {
            AddStep("Toggle chat overlay", () => chatOverlay.Show());
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(channel2));
            AddStep("Switch to channel 2", () => clickDrawable(chatOverlay.AvailableTabs.First()));
            AddStep("Close channel 2", () => clickDrawable(chatOverlay.AvailableTabs.First().CloseButton.Child));
            AddAssert("Selector remained closed", () => chatOverlay.SelectionOverlayState == Visibility.Hidden);
            AddAssert("Current channel is channel 2", () => channelManager.CurrentChannel.Value == channel1);
            AddStep("Close channel 1", () => clickDrawable(chatOverlay.AvailableTabs.First().CloseButton.Child));
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

            public IEnumerable<TestChannelTabItem> AvailableTabs => ((TestTabControl)ChannelTabControl).AvailableTabs();
        }

        private class TestTabControl : ChannelTabControl
        {
            protected override TabItem<Channel> CreateTabItem(Channel value) => new TestChannelTabItem(value) { OnRequestClose = TabCloseRequested };

            public IEnumerable<TestChannelTabItem> AvailableTabs()
            {
                foreach (var tab in TabContainer)
                {
                    if (!(tab is ChannelSelectorTabItem))
                        yield return (TestChannelTabItem)tab;
                }
            }
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
