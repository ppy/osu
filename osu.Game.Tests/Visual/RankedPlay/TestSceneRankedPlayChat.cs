// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Chat;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayChat : MultiplayerTestScene
    {
        private ChannelManager channelManager = null!;
        private Channel testChannel = null!;
        private int channelIdSequence;
        private int messageIdSequence;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var api = parent.Get<IAPIProvider>();
            Add(channelManager = new ChannelManager(api));

            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.Cache(channelManager);

            return dependencies;
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            messageIdSequence = 0;
            testChannel = channelManager.JoinChannel(new Channel { Id = ++channelIdSequence, Type = ChannelType.Multiplayer });
        });

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.RankedPlay);
                room.ChannelId = channelIdSequence;
                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 2 }));

            AddStep("load screen", () => LoadScreen(new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
        }

        [Test]
        public void TestFocusChat()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            AddUntilStep("chat not focused", () => this.ChildrenOfType<StandAloneChatDisplay.ChatTextBox>().SingleOrDefault()?.HasFocus, () => Is.False);

            AddStep("press enter", () => InputManager.Key(Key.Enter));

            AddUntilStep("chat is focused", () => this.ChildrenOfType<StandAloneChatDisplay.ChatTextBox>().Single().HasFocus, () => Is.True);

            AddStep("press escape", () => InputManager.Key(Key.Escape));

            AddUntilStep("chat not focused", () => this.ChildrenOfType<StandAloneChatDisplay.ChatTextBox>().SingleOrDefault()?.HasFocus, () => Is.False);
        }

        [Test]
        public void TestDiscardCardStage()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            postLocalUserMessage("this is a message from the local user");
            postOpponentMessage(
                "this is a message from the opponent. your opponent has a lot to say about you. nice stuff, of course. they see your potential in this game and want to shower you with compliments.");
        }

        [Test]
        public void TestResultsStage()
        {
            AddStep("set results state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.Results, state =>
            {
                int losingPlayer = state.Users.Keys.First();

                foreach (var (id, userInfo) in state.Users)
                {
                    if (id == losingPlayer)
                    {
                        userInfo.DamageInfo = new RankedPlayDamageInfo
                        {
                            RawDamage = 123_456,
                            Damage = 123_456,
                            OldLife = 500_000,
                            NewLife = 500_000 - 123_456,
                        };

                        userInfo.Life = 500_000 - 123_456;
                    }
                    else
                    {
                        userInfo.DamageInfo = new RankedPlayDamageInfo
                        {
                            RawDamage = 0,
                            Damage = 0,
                            OldLife = 1_000_000,
                            NewLife = 1_000_000,
                        };
                    }
                }
            }).WaitSafely());
        }

        [Test]
        public void TestReport()
        {
            ReportChatDialog dialog = null!;

            AddStep("setup request handling", () =>
            {
                dummyAPI.HandleRequest += request =>
                {
                    if (request is ChatReportRequest chatReportRequest)
                    {
                        chatReportRequest.TriggerSuccess();
                        return true;
                    }

                    return false;
                };
            });

            AddStep("set pick state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = API.LocalUser.Value.OnlineID).WaitSafely());
            postOpponentMessage("wangs");

            AddStep("show chat history", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<StandAloneChatDisplay.ChatTextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("right click message", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<OsuSpriteText>().First(t => t.Text == "wangs"));
                InputManager.Click(MouseButton.Right);
            });
            AddStep("Select report option", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<Menu.DrawableMenuItem>().First(m => m.Item.Text.ToString() == "Report"));
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("report dialog is present", () => (dialog = this.ChildrenOfType<ReportChatDialog>().Single()).IsPresent, () => Is.True);

            AddStep("input reason", () => dialog.ChildrenOfType<OsuTextBox>().First().Text = "reason");
            AddStep("send report", () => DialogOverlay.CurrentDialog!.PerformAction<ReportDialog<ChatReportReason>.SubmitButton>());

            AddUntilStep("Info message displayed", () => testChannel.Messages.Last(), Is.InstanceOf<InfoMessage>);
        }

        private void postLocalUserMessage(string content)
        {
            AddStep("add local user message", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Timestamp = DateTimeOffset.Now,
                Sender = API.LocalUser.Value,
                Content = content
            }));
        }

        private void postOpponentMessage(string content)
        {
            AddStep("add opponent message", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Timestamp = DateTimeOffset.Now,
                Sender = new APIUser
                {
                    Id = 2,
                    Username = "peppy"
                },
                Content = content
            }));
        }
    }
}
