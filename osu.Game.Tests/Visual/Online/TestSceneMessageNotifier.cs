﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
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
            if (API is DummyAPIAccess daa)
            {
                daa.HandleRequest = dummyAPIHandleRequest;
            }

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

        private bool dummyAPIHandleRequest(APIRequest request)
        {
            switch (request)
            {
                case GetMessagesRequest messagesRequest:
                    messagesRequest.TriggerSuccess(new List<Message>(0));
                    return true;

                case CreateChannelRequest createChannelRequest:
                    var apiChatChannel = new APIChatChannel
                    {
                        RecentMessages = new List<Message>(0),
                        ChannelID = (int)createChannelRequest.Channel.Id
                    };
                    createChannelRequest.TriggerSuccess(apiChatChannel);
                    return true;

                case ListChannelsRequest listChannelsRequest:
                    listChannelsRequest.TriggerSuccess(new List<Channel>(1) { publicChannel });
                    return true;

                case GetUpdatesRequest updatesRequest:
                    updatesRequest.TriggerSuccess(new GetUpdatesResponse
                    {
                        Messages = new List<Message>(0),
                        Presence = new List<Channel>(0)
                    });
                    return true;

                case JoinChannelRequest joinChannelRequest:
                    joinChannelRequest.TriggerSuccess();
                    return true;

                default:
                    return false;
            }
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
            [Cached]
            public ChannelManager ChannelManager { get; } = new ChannelManager();

            [Cached]
            public NotificationOverlay NotificationOverlay { get; } = new NotificationOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            [Cached]
            public ChatOverlay ChatOverlay { get; } = new ChatOverlay();

            private readonly MessageNotifier messageNotifier = new MessageNotifier();

            private readonly Channel[] channels;

            public TestContainer(Channel[] channels)
            {
                this.channels = channels;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    ChannelManager,
                    ChatOverlay,
                    NotificationOverlay,
                    messageNotifier,
                };

                ((BindableList<Channel>)ChannelManager.AvailableChannels).AddRange(channels);

                foreach (var channel in channels)
                    ChannelManager.JoinChannel(channel);
            }
        }
    }
}
