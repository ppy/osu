// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
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
            AddRepeatStep("post message", () => history.PostMessage(new APIUser { Id = 2 }, $"message {messageId++}"), 20);
        }

        [Test]
        public void TestCollapse()
        {
            AddStep("set expanded", () => history.Expand());

            AddStep("post some messages", () =>
            {
                for (int i = 0; i < 10; i++)
                    history.PostMessage(new APIUser { Id = 2 }, $"message {i}");
            });

            AddWaitStep("wait a bit", 10);
            AddStep("set collapsed", () => history.Collapse());
            AddWaitStep("wait a bit", 10);
            AddStep("set expanded", () => history.Expand());
            AddWaitStep("wait a bit", 10);
            AddStep("set collapsed", () => history.Collapse());
        }
    }
}
