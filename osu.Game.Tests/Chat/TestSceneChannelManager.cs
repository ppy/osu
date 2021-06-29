// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;
using osu.Game.Tests.Visual;
using osu.Game.Users;

namespace osu.Game.Tests.Chat
{
    [HeadlessTest]
    public class TestSceneChannelManager : OsuTestScene
    {
        private ChannelManager channelManager;
        private int currentMessageId;

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

                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case JoinChannelRequest _:
                            return true;

                        case PostMessageRequest postMessage:
                            postMessage.TriggerSuccess(new Message(++currentMessageId)
                            {
                                IsAction = postMessage.Message.IsAction,
                                ChannelId = postMessage.Message.ChannelId,
                                Content = postMessage.Message.Content,
                                Links = postMessage.Message.Links,
                                Timestamp = postMessage.Message.Timestamp,
                                Sender = postMessage.Message.Sender
                            });

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

        private Channel createChannel(int id, ChannelType type) => new Channel(new User())
        {
            Id = id,
            Name = $"Channel {id}",
            Topic = $"Topic of channel {id} with type {type}",
            Type = type,
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
