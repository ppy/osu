// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public class TestSceneTournamentMatchChatDisplay : OsuTestScene
    {
        private readonly Channel testChannel = new Channel();
        private readonly Channel testChannel2 = new Channel();

        private readonly APIUser admin = new APIUser
        {
            Username = "HappyStick",
            Id = 2,
            Colour = "f2ca34"
        };

        private readonly APIUser redUser = new APIUser
        {
            Username = "BanchoBot",
            Id = 3,
        };

        private readonly APIUser blueUser = new APIUser
        {
            Username = "Zallius",
            Id = 4,
        };

        [Cached]
        private LadderInfo ladderInfo = new LadderInfo();

        [Cached]
        private MatchIPCInfo matchInfo = new MatchIPCInfo(); // hide parent

        private readonly TournamentMatchChatDisplay chatDisplay;

        public TestSceneTournamentMatchChatDisplay()
        {
            Add(chatDisplay = new TournamentMatchChatDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            ladderInfo.CurrentMatch.Value = new TournamentMatch
            {
                Team1 =
                {
                    Value = new TournamentTeam { Players = new BindableList<APIUser> { redUser } }
                },
                Team2 =
                {
                    Value = new TournamentTeam { Players = new BindableList<APIUser> { blueUser } }
                }
            };

            chatDisplay.Channel.Value = testChannel;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = admin,
                Content = "I am a wang!"
            }));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser,
                Content = "I am team red."
            }));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser,
                Content = "I plan to win!"
            }));

            AddStep("message from team blue", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = blueUser,
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = admin,
                Content = "Okay okay, calm down guys. Let's do this!"
            }));

            AddStep("multiple messages", () => testChannel.AddNewMessages(
                new Message(nextMessageId())
                {
                    Sender = admin,
                    Content = "I spam you!"
                },
                new Message(nextMessageId())
                {
                    Sender = admin,
                    Content = "I spam you!!!1"
                },
                new Message(nextMessageId())
                {
                    Sender = admin,
                    Content = "I spam you!1!1"
                }));

            AddStep("change channel to 2", () => chatDisplay.Channel.Value = testChannel2);

            AddStep("change channel to 1", () => chatDisplay.Channel.Value = testChannel);
        }

        private int messageId;

        private long? nextMessageId() => messageId++;
    }
}
