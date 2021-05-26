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
    public class TestSceneMessageNotifier : OsuManualInputManagerTestScene
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
            AddStep("switch to PMs", () => testContainer.ChannelManager.CurrentChannel.Value = privateMessageChannel);

            AddStep("receive public message", () => receiveMessage(friend, publicChannel, "Hello everyone"));
            AddAssert("no notifications fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);

            AddStep("receive message containing mention", () => receiveMessage(friend, publicChannel, $"Hello {API.LocalUser.Value.Username.ToLowerInvariant()}!"));
            AddAssert("1 notification fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);

            AddStep("open notification overlay", () => testContainer.NotificationOverlay.Show());
            AddStep("click notification", clickNotification<MessageNotifier.MentionNotification>);

            AddAssert("chat overlay is open", () => testContainer.ChatOverlay.State.Value == Visibility.Visible);
            AddAssert("public channel is selected", () => testContainer.ChannelManager.CurrentChannel.Value == publicChannel);
        }

        [Test]
        public void TestPrivateMessageNotification()
        {
            AddStep("switch to public channel", () => testContainer.ChannelManager.CurrentChannel.Value = publicChannel);

            AddStep("receive PM", () => receiveMessage(friend, privateMessageChannel, $"Hello {API.LocalUser.Value.Username}"));
            AddAssert("1 notification fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);

            AddStep("open notification overlay", () => testContainer.NotificationOverlay.Show());
            AddStep("click notification", clickNotification<MessageNotifier.PrivateMessageNotification>);

            AddAssert("chat overlay is open", () => testContainer.ChatOverlay.State.Value == Visibility.Visible);
            AddAssert("PM channel is selected", () => testContainer.ChannelManager.CurrentChannel.Value == privateMessageChannel);
        }

        [Test]
        public void TestNoNotificationWhenPMChannelOpen()
        {
            AddStep("switch to PMs", () => testContainer.ChannelManager.CurrentChannel.Value = privateMessageChannel);

            AddStep("receive PM", () => receiveMessage(friend, privateMessageChannel, "you're reading this, right?"));

            AddAssert("no notifications fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);
        }

        [Test]
        public void TestNoNotificationWhenMentionedInOpenPublicChannel()
        {
            AddStep("switch to public channel", () => testContainer.ChannelManager.CurrentChannel.Value = publicChannel);

            AddStep("receive mention", () => receiveMessage(friend, publicChannel, $"{API.LocalUser.Value.Username.ToUpperInvariant()} has been reading this"));

            AddAssert("no notifications fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);
        }

        [Test]
        public void TestNoNotificationOnSelfMention()
        {
            AddStep("switch to PM channel", () => testContainer.ChannelManager.CurrentChannel.Value = privateMessageChannel);

            AddStep("receive self-mention", () => receiveMessage(API.LocalUser.Value, publicChannel, $"my name is {API.LocalUser.Value.Username}"));

            AddAssert("no notifications fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);
        }

        [Test]
        public void TestNoNotificationOnPMFromSelf()
        {
            AddStep("switch to public channel", () => testContainer.ChannelManager.CurrentChannel.Value = publicChannel);

            AddStep("receive PM from self", () => receiveMessage(API.LocalUser.Value, privateMessageChannel, "hey hey"));

            AddAssert("no notifications fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);
        }

        [Test]
        public void TestNotificationsNotFiredTwice()
        {
            AddStep("switch to public channel", () => testContainer.ChannelManager.CurrentChannel.Value = publicChannel);

            AddStep("receive same PM twice", () =>
            {
                var message = createMessage(friend, privateMessageChannel, "hey hey");
                privateMessageChannel.AddNewMessages(message, message);
            });

            AddStep("open notification overlay", () => testContainer.NotificationOverlay.Show());
            AddAssert("1 notification fired", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);
        }

        private void receiveMessage(User sender, Channel channel, string content) => channel.AddNewMessages(createMessage(sender, channel, content));

        private Message createMessage(User sender, Channel channel, string content) => new Message(messageIdCounter++)
        {
            Content = content,
            Sender = sender,
            ChannelId = channel.Id
        };

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

                foreach (var channel in channels)
                    ChannelManager.JoinChannel(channel);
            }
        }
    }
}
