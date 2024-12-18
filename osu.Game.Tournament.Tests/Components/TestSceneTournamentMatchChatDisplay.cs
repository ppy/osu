// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Tests.Components
{
    public partial class TestSceneTournamentMatchChatDisplay : OsuTestScene
    {
        private readonly Channel testChannel = new Channel();
        private readonly Channel testChannel2 = new Channel();

        private readonly APIUser admin = new APIUser
        {
            Username = "HappyStick",
            Id = 2,
            Colour = "f2ca34"
        };

        private readonly TournamentUser redUser = new TournamentUser
        {
            Username = "BanchoBot",
            OnlineID = 3,
        };

        private readonly TournamentUser blueUser = new TournamentUser
        {
            Username = "Zallius",
            OnlineID = 4,
        };

        private readonly TournamentUser blueUserWithCustomColour = new TournamentUser
        {
            Username = "nekodex",
            OnlineID = 5,
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

            AddStep("set current match", () => ladderInfo.CurrentMatch.Value = new TournamentMatch
            {
                Team1 =
                {
                    Value = new TournamentTeam { Players = { redUser } }
                },
                Team2 =
                {
                    Value = new TournamentTeam { Players = { blueUser, blueUserWithCustomColour } }
                }
            });

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser.ToAPIUser(),
                Content = "I am team red."
            }));

            AddUntilStep("message from team red is red color", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().AccentColour, () => Is.EqualTo(TournamentGame.COLOUR_RED));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUser.ToAPIUser(),
                Content = "I plan to win!"
            }));

            AddStep("message from team blue", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = blueUser.ToAPIUser(),
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddUntilStep("message from team blue is blue color", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().AccentColour, () => Is.EqualTo(TournamentGame.COLOUR_BLUE));

            var userWithCustomColour = blueUserWithCustomColour.ToAPIUser();
            userWithCustomColour.Colour = "#e45678";

            AddStep("message from team blue with custom colour", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = userWithCustomColour,
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddUntilStep("message from team blue is blue color", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().AccentColour, () => Is.EqualTo(TournamentGame.COLOUR_BLUE));

            AddUntilStep("message from user with custom colour is inverted", () =>
                this.ChildrenOfType<DrawableChatUsername>().Last().Inverted, () => Is.EqualTo(true));

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
