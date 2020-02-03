// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneMessageNotifier : ManualInputManagerTestScene
    {
        private User friend;
        private Channel publicChannel;
        private Channel privateMessageChannel;
        private TestContainer testContainer;

        private int messageIdCounter;

        [SetUp]
        public void Setup()
        {
            friend = new User { Id = 0, Username = "Friend" };
            publicChannel = new Channel { Id = 1, Name = "osu" };
            privateMessageChannel = new Channel(friend) { Id = 2, Name = friend.Username, Type = ChannelType.PM };

            Schedule(() =>
            {
                Child = testContainer = new TestContainer(new[] { publicChannel, privateMessageChannel })
                {
                    RelativeSizeAxes = Axes.Both,
                };

                testContainer.ChatOverlay.Show();
            });
        }

        [Test]
        public void TestPublicChannelMention()
        {
            AddStep("Switch to PMs", () => testContainer.ChannelManager.CurrentChannel.Value = privateMessageChannel);

            AddStep("Send regular message", () => publicChannel.AddNewMessages(new Message(messageIdCounter++) { Content = "Hello everyone!", Sender = friend, ChannelId = publicChannel.Id }));
            AddAssert("Expect no notifications", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);

            AddStep("Send message containing mention", () => publicChannel.AddNewMessages(new Message(messageIdCounter++) { Content = $"Hello {API.LocalUser.Value.Username.ToLowerInvariant()}!", Sender = friend, ChannelId = publicChannel.Id }));
            AddAssert("Expect 1 notification", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);

            AddStep("Open notification overlay", () => testContainer.NotificationOverlay.Show());
            AddStep("Click notification", clickNotification<MessageNotifier.MentionNotification>);

            AddAssert("Expect ChatOverlay is open", () => testContainer.ChatOverlay.State.Value == Visibility.Visible);
            AddAssert("Expect the public channel to be selected", () => testContainer.ChannelManager.CurrentChannel.Value == publicChannel);
        }

        [Test]
        public void TestPrivateMessageNotification()
        {
            AddStep("Switch to public channel", () => testContainer.ChannelManager.CurrentChannel.Value = publicChannel);

            AddStep("Send PM", () => privateMessageChannel.AddNewMessages(new Message(messageIdCounter++) { Content = $"Hello {API.LocalUser.Value.Username}!", Sender = friend, ChannelId = privateMessageChannel.Id }));
            AddAssert("Expect 1 notification", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);

            AddStep("Open notification overlay", () => testContainer.NotificationOverlay.Show());
            AddStep("Click notification", clickNotification<MessageNotifier.PrivateMessageNotification>);

            AddAssert("Expect ChatOverlay is open", () => testContainer.ChatOverlay.State.Value == Visibility.Visible);
            AddAssert("Expect the PM channel to be selected", () => testContainer.ChannelManager.CurrentChannel.Value == privateMessageChannel);
        }

        private void clickNotification<T>() where T : Notification
        {
            var notification = testContainer.NotificationOverlay.ChildrenOfType<T>().Single();

            InputManager.MoveMouseTo(notification);
            InputManager.Click(MouseButton.Left);
        }

        private class TestContainer : Container
        {
            private readonly Channel[] channels;

            public TestContainer(Channel[] channels) => this.channels = channels;

            [Cached]
            public ChannelManager ChannelManager { get; } = new ChannelManager();

            [Cached]
            public NotificationOverlay NotificationOverlay { get; } = new NotificationOverlay();

            [Cached]
            public MessageNotifier MessageNotifier { get; } = new MessageNotifier();

            [Cached]
            public ChatOverlay ChatOverlay { get; } = new ChatOverlay();

            [BackgroundDependencyLoader]
            private void load()
            {
                AddRange(new Drawable[] { ChannelManager, ChatOverlay, NotificationOverlay, MessageNotifier });

                ((BindableList<Channel>)ChannelManager.AvailableChannels).AddRange(channels);
                ((BindableList<Channel>)ChannelManager.JoinedChannels).AddRange(channels);
            }
        }
    }
}
