// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneMessageNotifier : OsuTestScene
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
            privateMesssageChannel = new Channel(friend) { Id = 2, Name = friend.Username, Type = ChannelType.PM };

            Schedule(() =>
            {
                Child = testContainer = new TestContainer(new[] { publicChannel, privateMesssageChannel })
                {
                    RelativeSizeAxes = Axes.Both,
                };

                testContainer.ChatOverlay.Show();
            });
        }

        [Test]
        public void TestPublicChannelMention()
        {
            AddStep("Switch to PMs", () => testContainer.ChannelManager.CurrentChannel.Value = privateMesssageChannel);

            AddStep("Send regular message", () => publicChannel.AddNewMessages(new Message(messageIdCounter++) { Content = "Hello everyone!", Sender = friend, ChannelId = publicChannel.Id }));
            AddAssert("Expect no notifications", () => testContainer.NotificationOverlay.UnreadCount.Value == 0);

            AddStep("Send message containing mention", () => publicChannel.AddNewMessages(new Message(messageIdCounter++) { Content = $"Hello {API.LocalUser.Value.Username.ToLowerInvariant()}!", Sender = friend, ChannelId = publicChannel.Id }));
            AddAssert("Expect 1 notification", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);
        }

        [Test]
        public void TestPrivateMessageNotification()
        {
            AddStep("Send PM", () => privateMesssageChannel.AddNewMessages(new Message(messageIdCounter++) { Content = $"Hello {API.LocalUser.Value.Username}!", Sender = friend, ChannelId = privateMesssageChannel.Id }));
            AddAssert("Expect 1 notification", () => testContainer.NotificationOverlay.UnreadCount.Value == 1);
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
                AddRange(new Drawable[] { ChannelManager, NotificationOverlay });

                ((BindableList<Channel>)ChannelManager.AvailableChannels).AddRange(channels);

                AddRange(new Drawable[] { ChatOverlay, MessageNotifier });

                ((BindableList<Channel>)ChannelManager.JoinedChannels).AddRange(channels);
            }
        }
    }
}
