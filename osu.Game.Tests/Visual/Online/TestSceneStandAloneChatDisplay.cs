// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;
using System;
using System.Linq;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneStandAloneChatDisplay : OsuTestScene
    {
        private readonly Channel testChannel = new Channel();

        private readonly User admin = new User
        {
            Username = "HappyStick",
            Id = 2,
            Colour = "f2ca34"
        };

        private readonly User redUser = new User
        {
            Username = "BanchoBot",
            Id = 3,
        };

        private readonly User blueUser = new User
        {
            Username = "Zallius",
            Id = 4,
        };

        private readonly User longUsernameUser = new User
        {
            Username = "Very Long Long Username",
            Id = 5,
        };

        [Cached]
        private ChannelManager channelManager = new ChannelManager();

        private readonly TestStandAloneChatDisplay chatDisplay;
        private readonly TestStandAloneChatDisplay chatDisplay2;

        public TestSceneStandAloneChatDisplay()
        {
            Add(channelManager);

            Add(chatDisplay = new TestStandAloneChatDisplay
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding(20),
                Size = new Vector2(400, 80)
            });

            Add(chatDisplay2 = new TestStandAloneChatDisplay(true)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Margin = new MarginPadding(20),
                Size = new Vector2(400, 150)
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            channelManager.CurrentChannel.Value = testChannel;

            chatDisplay.Channel.Value = testChannel;
            chatDisplay2.Channel.Value = testChannel;

            int sequence = 0;

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = admin,
                Content = "I am a wang!"
            }));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = redUser,
                Content = "I am team red."
            }));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = redUser,
                Content = "I plan to win!"
            }));

            AddStep("message from team blue", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = blueUser,
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = admin,
                Content = "Okay okay, calm down guys. Let's do this!"
            }));

            AddStep("message from long username", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = longUsernameUser,
                Content = "Hi guys, my new username is lit!"
            }));

            AddStep("message with new date", () => testChannel.AddNewMessages(new Message(sequence++)
            {
                Sender = longUsernameUser,
                Content = "Message from the future!",
                Timestamp = DateTimeOffset.Now
            }));

            AddUntilStep("ensure still scrolled to bottom", () => chatDisplay.ScrolledToBottom);

            const int messages_per_call = 10;
            AddRepeatStep("add many messages", () =>
                {
                    for (int i = 0; i < messages_per_call; i++)
                        testChannel.AddNewMessages(new Message(sequence++)
                        {
                            Sender = longUsernameUser,
                            Content = "Many messages! " + Guid.NewGuid(),
                            Timestamp = DateTimeOffset.Now
                        });
                }, Channel.MAX_HISTORY / messages_per_call + 5);

            AddAssert("Ensure no adjacent day separators", () =>
            {
                var indices = chatDisplay.FillFlow.OfType<DrawableChannel.DaySeparator>().Select(ds => chatDisplay.FillFlow.IndexOf(ds));

                foreach (var i in indices)
                    if (i < chatDisplay.FillFlow.Count && chatDisplay.FillFlow[i + 1] is DrawableChannel.DaySeparator)
                        return false;

                return true;
            });

            AddUntilStep("ensure still scrolled to bottom", () => chatDisplay.ScrolledToBottom);
        }

        private class TestStandAloneChatDisplay : StandAloneChatDisplay
        {
            public TestStandAloneChatDisplay(bool textbox = false)
                : base(textbox)
            {
            }

            protected DrawableChannel DrawableChannel => InternalChildren.OfType<DrawableChannel>().First();

            protected OsuScrollContainer ScrollContainer => (OsuScrollContainer)((Container)DrawableChannel.Child).Child;

            public FillFlowContainer FillFlow => (FillFlowContainer)ScrollContainer.Child;

            public bool ScrolledToBottom => ScrollContainer.IsScrolledToEnd(1);
        }
    }
}
