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
using osuTK;

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
            Colour = "f2ca34",
        };

        private readonly TournamentUser carbonReferee = new TournamentUser
        {
            Username = "Sh0rtD011y",
            OnlineID = 114514,
        };

        private readonly TournamentUser cyberReferee = new TournamentUser
        {
            Username = "Juroe",
            OnlineID = 1919810,
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

        private readonly TournamentUser redUserWithLongName = new TournamentUser
        {
            Username = "YouKnowWhatImGoingToDo",
            OnlineID = 6,
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
                    Value = new TournamentTeam { Players = { redUser, redUserWithLongName } }
                },
                Team2 =
                {
                    Value = new TournamentTeam { Players = { blueUser, blueUserWithCustomColour } }
                },
                Round =
                {
                    Value = new TournamentRound
                    {
                        Referees = { carbonReferee, cyberReferee }
                    }
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

            AddStep("message with a long username", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = redUserWithLongName.ToAPIUser(),
                Content = "I have said this before that Genshin Impact is an action game, and I forgot the remaining part ;w;",
            }));

            AddStep("really long message", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = admin,
                Content = "你说的对，但是《原神》是由米哈游自主研发的一款全新开放世界冒险游戏。游戏发生在一个被称作「提瓦特」的幻想世界，在这里，被神选中的人将被授予「神之眼」，导引元素之力。你将扮演一位名为「旅行者」的神秘角色在自由的旅行中邂逅性格各异、能力独特的同伴们，和他们一起击败强敌，找回失散的亲人——同时，逐步发掘「原神」的真相。我现在每天玩原神都能赚150原石，每个月差不多5000原石的收入，也就是现实生活中每个月5000美元的收入水平，换算过来最少也30000人民币，虽然我只有14岁，但是已经超越了绝大多数人的水平，这便是原神给我的骄傲的资本。毫不夸张地说，《原神》是miHoYo迄今为止规模最为宏大，也是最具野心的一部作品。即便在经历了8700个小时的艰苦战斗后，游戏还有许多尚未发现的秘密，错过的武器与装备，以及从未使用过的法术和技能。\n尽管游戏中的战斗体验和我们之前在烧机系列游戏所见到的没有多大差别，但游戏中各类精心设计的敌人以及Boss战已然将战斗抬高到了一个全新的水平。就和几年前的《塞尔达传说》一样，《原神》也是一款能够推动同类游戏向前发展的优秀作品。",
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

            AddStep("referee messages", () => testChannel2.AddNewMessages(new Message(nextMessageId())
            {
                Sender = cyberReferee.ToAPIUser(),
                Content = "大家好啊，我是说的道理",
            }));

            AddStep("referee commands", () => testChannel2.AddNewMessages(new Message(nextMessageId())
            {
                Sender = carbonReferee.ToAPIUser(),
                Content = "[*] 比赛时间已到，请各位选手启动原神",
            }));

            AddStep("non-referee commands", () => testChannel2.AddNewMessages(new Message(nextMessageId())
            {
                Sender = blueUser.ToAPIUser(),
                Content = "[*] 大家好啊，我是说的老鲤",
            }));

            AddStep("change channel to 1", () => chatDisplay.Channel.Value = testChannel);

            AddStep("resize container to 500x500 with animation", () =>
            {
                chatDisplay.RelativeSizeAxes = Axes.None;
                chatDisplay.ResizeTo(new Vector2(500, 500), 1000, Easing.InOutQuint);
            });

            AddUntilStep("chat display don't use relative size", () =>
                chatDisplay.RelativeSizeAxes == Axes.None, () => Is.EqualTo(true));
        }

        private int messageId;

        private long? nextMessageId() => messageId++;
    }
}
