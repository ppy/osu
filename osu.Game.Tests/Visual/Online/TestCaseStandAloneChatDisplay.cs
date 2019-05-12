// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestCaseStandAloneChatDisplay : OsuTestCase
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

        [Cached]
        private ChannelManager channelManager = new ChannelManager();

        private readonly StandAloneChatDisplay chatDisplay;
        private readonly StandAloneChatDisplay chatDisplay2;

        public TestCaseStandAloneChatDisplay()
        {
            Add(channelManager);

            Add(chatDisplay = new StandAloneChatDisplay
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Margin = new MarginPadding(20),
                Size = new Vector2(400, 80)
            });

            Add(chatDisplay2 = new StandAloneChatDisplay(true)
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
        }
    }
}
