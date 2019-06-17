// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Ladder.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tournament.Tests
{
    public class TestSceneMatchChatDisplay : OsuTestScene
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
        private LadderInfo ladderInfo = new LadderInfo();

        [Cached]
        private MatchIPCInfo matchInfo = new MatchIPCInfo(); // hide parent

        public TestSceneMatchChatDisplay()
        {
            MatchChatDisplay chatDisplay;

            Add(chatDisplay = new MatchChatDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(400, 80)
            });

            ladderInfo.CurrentMatch.Value = new MatchPairing
            {
                Team1 =
                {
                    Value = new TournamentTeam { Players = new BindableList<User> { redUser } }
                },
                Team2 =
                {
                    Value = new TournamentTeam { Players = new BindableList<User> { blueUser } }
                }
            };

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
