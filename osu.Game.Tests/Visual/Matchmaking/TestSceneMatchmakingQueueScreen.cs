// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking.Intro;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Users;

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

            AddStep("load screen", () => LoadScreen(new ScreenIntro()));
        }

        [Test]
        public void TestBasic()
        {
            AddUntilStep("wait for queue screen", () => queueScreen?.IsLoaded == true);

            AddStep("set users", () =>
            {
                queueScreen!.Users = Enumerable.Range(0, 10).Select(_ => new APIUser
                {
                    Username = "peppy",
                    Statistics = new UserStatistics { GlobalRank = 1234 },
                    Id = RNG.Next(2, 30000000),
                }).ToArray();
            });

            AddStep("change state to idle", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.Idle));

            AddStep("change state to queueing", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.Queueing));

            AddStep("change state to found match", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.PendingAccept));

            AddStep("change state to waiting for room", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.AcceptedWaitingForRoom));

            AddStep("change state to in room", () => queueScreen!.SetState(ScreenQueue.MatchmakingScreenState.InRoom));
        }
    }
}
