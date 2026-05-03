// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChatTicker : OsuManualInputManagerTestScene
    {
        private APIUser friend;
        private APIUser importantPerson;
        private Channel publicChannel;
        private Channel privateMessageChannel;
        private TestContainer testContainer;

        private int messageIdCounter;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            if (API is DummyAPIAccess daa)
            {
                daa.HandleRequest = dummyAPIHandleRequest;
            }

            friend = new APIUser { Id = 0, Username = "SomeFriend" };
            importantPerson = new APIUser { Username = @"i-am-important", Id = 42, Colour = "#250cc9" };
            publicChannel = new Channel { Id = 1, Name = "#osu" };
            privateMessageChannel = new Channel(friend) { Id = 2, Name = friend.Username, Type = ChannelType.PM };

            Schedule(() =>
            {
                Child = testContainer = new TestContainer(API, new[] { publicChannel, privateMessageChannel })
                {
                    RelativeSizeAxes = Axes.Both,
                };
            });
        });

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
        public void TestChatTicker()
        {
            AddStep("switch to public channel", () => testContainer.ChannelManager.CurrentChannel.Value = publicChannel);

            AddStep("receive public message", () => receiveMessage(friend, publicChannel, "Hello everyone"));

            AddStep("receive message containing mention", () => receiveMessage(friend, publicChannel, $"Hello {API.LocalUser.Value.Username.ToLowerInvariant()}!"));

            AddStep("receive message from VIP", () => receiveMessage(importantPerson, publicChannel, "Hello everyone!"));

            AddStep("receive message from VIP containing mention", () => receiveMessage(importantPerson, publicChannel, $"Hello {API.LocalUser.Value.Username.ToLowerInvariant()}!"));

            AddStep("receive very long message", () => receiveMessage(importantPerson, publicChannel, string.Concat(Enumerable.Repeat("Hello everyone! ", 50))));

            AddToggleStep("toggle show ticker", b => config.SetValue(OsuSetting.ChatTicker, b));
        }

        private void receiveMessage(APIUser sender, Channel channel, string content) => channel.AddNewMessages(createMessage(sender, channel, content));

        private Message createMessage(APIUser sender, Channel channel, string content) => new Message(messageIdCounter++)
        {
            Content = content,
            Sender = sender,
            ChannelId = channel.Id
        };

        private partial class TestContainer : Container
        {
            [Cached]
            public ChannelManager ChannelManager { get; }

            [Cached(typeof(INotificationOverlay))]
            public NotificationOverlay NotificationOverlay { get; } = new NotificationOverlay
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            [Cached]
            public ChatOverlay ChatOverlay { get; } = new ChatOverlay();

            [Cached]
            public ChatTicker ChatTicker { get; } = new ChatTicker();

            private readonly MessageNotifier messageNotifier = new MessageNotifier();

            private readonly Channel[] channels;

            public TestContainer(IAPIProvider api, Channel[] channels)
            {
                this.channels = channels;
                ChannelManager = new ChannelManager(api);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    ChannelManager,
                    ChatOverlay,
                    ChatTicker,
                    messageNotifier,
                };

                ((BindableList<Channel>)ChannelManager.AvailableChannels).AddRange(channels);

                foreach (var channel in channels)
                    ChannelManager.JoinChannel(channel);
            }
        }
    }
}
