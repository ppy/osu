// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Chat
{
    [HeadlessTest]
    public class TestSceneChannelManager : OsuTestScene
    {
        private ChannelManager channelManager;
        private int currentMessageId;
        private List<Message> sentMessages;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            var container = new ChannelManagerContainer();
            Child = container;
            channelManager = container.ChannelManager;
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("register request handling", () =>
            {
                currentMessageId = 0;
                sentMessages = new List<Message>();

                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case JoinChannelRequest joinChannel:
                            joinChannel.TriggerSuccess();
                            return true;

                        case PostMessageRequest postMessage:
                            handlePostMessageRequest(postMessage);
                            return true;

                        case MarkChannelAsReadRequest markRead:
                            handleMarkChannelAsReadRequest(markRead);
                            return true;
                    }

                    return false;
                };
            });
        }

        [Test]
        public void TestCommandsPostedToCorrectChannelWhenNotCurrent()
        {
            Channel channel1 = null;
            Channel channel2 = null;

            AddStep("join 2 rooms", () =>
            {
                channelManager.JoinChannel(channel1 = createChannel(1, ChannelType.Public));
                channelManager.JoinChannel(channel2 = createChannel(2, ChannelType.Public));
            });

            AddStep("select channel 1", () => channelManager.CurrentChannel.Value = channel1);

            AddStep("post /me command to channel 2", () => channelManager.PostCommand("me dances", channel2));
            AddAssert("/me command received by channel 2", () => channel2.Messages.Last().Content == "dances");

            AddStep("post /np command to channel 2", () => channelManager.PostCommand("np", channel2));
            AddAssert("/np command received by channel 2", () => channel2.Messages.Last().Content.Contains("is listening to"));
        }

        [Test]
        public void TestMarkAsReadIgnoringLocalMessages()
        {
            Channel channel = null;

            AddStep("join channel and select it", () =>
            {
                channelManager.JoinChannel(channel = createChannel(1, ChannelType.Public));
                channelManager.CurrentChannel.Value = channel;
            });

            AddStep("post message", () => channelManager.PostMessage("Something interesting"));

            AddStep("post /help command", () => channelManager.PostCommand("help", channel));
            AddStep("post /me command with no action", () => channelManager.PostCommand("me", channel));
            AddStep("post /join command with no channel", () => channelManager.PostCommand("join", channel));
            AddStep("post /join command with non-existent channel", () => channelManager.PostCommand("join i-dont-exist", channel));
            AddStep("post non-existent command", () => channelManager.PostCommand("non-existent-cmd arg", channel));

            AddStep("mark channel as read", () => channelManager.MarkChannelAsRead(channel));
            AddAssert("channel's last read ID is set to the latest message", () => channel.LastReadId == sentMessages.Last().Id);
        }

        private void handlePostMessageRequest(PostMessageRequest request)
        {
            var message = new Message(++currentMessageId)
            {
                IsAction = request.Message.IsAction,
                ChannelId = request.Message.ChannelId,
                Content = request.Message.Content,
                Links = request.Message.Links,
                Timestamp = request.Message.Timestamp,
                Sender = request.Message.Sender
            };

            sentMessages.Add(message);
            request.TriggerSuccess(message);
        }

        private void handleMarkChannelAsReadRequest(MarkChannelAsReadRequest request)
        {
            // only accept messages that were sent through the API
            if (sentMessages.Contains(request.Message))
            {
                request.TriggerSuccess();
            }
            else
            {
                request.TriggerFailure(new APIException("unknown message!", null));
            }
        }

        private Channel createChannel(int id, ChannelType type) => new Channel(new APIUser())
        {
            Id = id,
            Name = $"Channel {id}",
            Topic = $"Topic of channel {id} with type {type}",
            Type = type,
            LastMessageId = 0,
        };

        private class ChannelManagerContainer : CompositeDrawable
        {
            [Cached]
            public ChannelManager ChannelManager { get; } = new ChannelManager();

            public ChannelManagerContainer()
            {
                InternalChild = ChannelManager;
            }
        }
    }
}
