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
            typeof(ChannelManager)
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

        [Test]
        public void TestLeaveChannelThenJoiningNew()
        {
            AddStep("Toggle chat overlay", () => chatOverlay.Show());
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddStep("Close channel 1", () => clickDrawable(chatOverlay.AvailableTabs.First().CloseButton.Child));
            AddAssert("Current channel is null", () => channelManager.CurrentChannel.Value == null);
            AddStep("Join channel 1", () => channelManager.JoinChannel(channel1));
            AddAssert("Current channel is channel 1", () => channelManager.CurrentChannel.Value == channel1);
        }

        private void clickDrawable(Drawable d)
        {
            InputManager.MoveMouseTo(d);
            InputManager.Click(MouseButton.Left);
        }

        private class TestChatOverlay : ChatOverlay
        {
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