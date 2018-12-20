// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual
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

        public TestCaseStandAloneChatDisplay()
        {
            StandAloneChatDisplay chatDisplay;

            Add(chatDisplay = new StandAloneChatDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(400, 80)
            });

            chatDisplay.Channel.Value = testChannel;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("message from admin", () => testChannel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = admin,
                Content = "I am a wang!"
            }));

            AddStep("message from team red", () => testChannel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = redUser,
                Content = "I am team red."
            }));

            AddStep("message from team red", () => testChannel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = redUser,
                Content = "I plan to win!"
            }));

            AddStep("message from team blue", () => testChannel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = blueUser,
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddStep("message from admin", () => testChannel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = admin,
                Content = "Okay okay, calm down guys. Let's do this!"
            }));
        }
    }
}
