// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayChat : MultiplayerTestScene
    {
        private ChannelManager channelManager = null!;
        private Channel testChannel = null!;
        private int messageIdSequence;

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
            testChannel = channelManager.JoinChannel(new Channel { Id = 1, Type = ChannelType.Multiplayer });
        });

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var room = CreateDefaultRoom(MatchType.RankedPlay);
                room.ChannelId = 1;
                JoinRoom(room);
            });

            WaitForJoined();

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 2 }));

            AddStep("load screen", () => LoadScreen(new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
        }

        [Test]
        public void TestDiscardCardStage()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            postLocalUserMessage("this is a message from the local user");
            postOpponentMessage("this is a message from the opponent");
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
