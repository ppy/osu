// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneBubbleChatHistory : OsuTestScene
    {
        private RankedPlayChatDisplay.BubbleChatHistory history = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = history = new RankedPlayChatDisplay.BubbleChatHistory
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.BottomCentre,
                Width = 300
            };
        });

        [Test]
        public void TestPostMessages()
        {
            int messageId = 1;
            AddRepeatStep("post message", () => history.PostMessage(new Message
            {
                Sender = new APIUser { Id = 2 },
                Content = $"message {messageId++}"
            }), 20);
        }

        [Test]
        public void TestCollapse()
        {
            AddStep("set expanded", () => history.Expand());

            AddStep("post some messages", () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    history.PostMessage(new Message
                    {
                        Sender = new APIUser { Id = 2 },
                        Content = $"message {i}"
                    });
                }
            });

            AddWaitStep("wait a bit", 10);
            AddStep("set collapsed", () => history.Collapse());
            AddWaitStep("wait a bit", 10);
            AddStep("set expanded", () => history.Expand());
            AddWaitStep("wait a bit", 10);
            AddStep("set collapsed", () => history.Collapse());
        }

        [Test]
        public void TestMessageDisappear()
        {
            AddStep("post a message", () => history.PostMessage(new Message
            {
                Sender = new APIUser { Id = 2 },
                Content = "message"
            }));

            AddWaitStep("wait a bit", 30);
            AddStep("set collapsed", () => history.Collapse());
            AddAssert("message bubble hidden", () => history.Messages.FirstOrDefault()?.Alpha == 0);
        }

        [Test]
        public void TestResolvePendingMessage()
        {
            var message = new Message(1)
            {
                Sender = new APIUser { Id = 2 },
                Content = "message",
            };
            var newMessage = new Message(1)
            {
                Sender = new APIUser { Id = 2 },
                Content = "new message",
            };
            AddStep("post a message", () => history.PostMessage(message));

            AddWaitStep("wait a bit", 10);
            AddStep("resolve pending message", () => history.ResolvePendingMessage(message, newMessage));
            AddAssert("resolved message text changed", () => history.Messages.FirstOrDefault()?.Message.Content == newMessage.Content);
        }
    }
}
