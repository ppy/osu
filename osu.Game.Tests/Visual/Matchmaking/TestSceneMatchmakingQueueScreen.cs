// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Intro;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingQueueScreen : MultiplayerTestScene
    {
        [Cached]
        private readonly QueueController controller = new QueueController();

        private ScreenQueue? queueScreen => Stack.CurrentScreen as ScreenQueue;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("load screen", () => LoadScreen(new ScreenIntro(MatchmakingPoolType.QuickPlay)));
            AddUntilStep("wait for queue screen", () => queueScreen?.IsLoaded == true);

            AddStep("send status update", () =>
            {
                int userId1 = Random.Shared.Next(1, 11);
                int userId2 = Random.Shared.GetItems(Enumerable.Range(1, 10).Except([userId1]).ToArray(), 1).Single();

                MultiplayerClient.MatchmakingLobbyStatusChanged(new MatchmakingLobbyStatus
                {
                    UsersInQueue = Enumerable.Range(1, 10).ToArray(),
                    RatingDistribution = Enumerable.Range(0, 24).Select(i => (400 + i * 100, (int)Math.Round(generateCount(400 + i * 100, 1600, 400, 7200)))).ToArray(),
                    UserRating = Random.Shared.Next(400, 2800),
                    RecentMatches = Enumerable.Range(1, 10).Select(_ => (MatchRoomState)new RankedPlayRoomState
                    {
                        Users =
                        {
                            { userId1, new RankedPlayUserInfo { Rating = 0, Life = Random.Shared.Next(0, 1_000_001), RoundsWon = Random.Shared.Next(0, 4) } },
                            { userId2, new RankedPlayUserInfo { Rating = 0, Life = Random.Shared.Next(0, 1_000_001), RoundsWon = Random.Shared.Next(0, 4) } },
                        }
                    }).ToArray()
                }).WaitSafely();
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("change state to idle", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.Idle));

            AddStep("change state to queueing", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.Queueing));

            AddStep("change state to found match", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.PendingAccept));

            AddStep("change state to waiting for room", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.AcceptedWaitingForRoom));

            AddStep("change state to in room", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.InRoom));
        }

        private static double generateCount(double x, double mean, double stdDev, double amplitude)
        {
            return amplitude * Math.Exp(-Math.Pow(x - mean, 2) / (2 * Math.Pow(stdDev, 2))) + Random.Shared.Next(300);
        }
    }
}
