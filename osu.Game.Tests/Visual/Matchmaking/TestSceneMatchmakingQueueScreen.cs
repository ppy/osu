// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneMatchmakingQueueScreen : MultiplayerTestScene
    {
        [Cached]
        private readonly MatchmakingController controller = new MatchmakingController();

        private MatchmakingQueueScreen? queueScreen => Stack.CurrentScreen as MatchmakingQueueScreen;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load screen", () => LoadScreen(new MatchmakingIntroScreen()));
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

            AddStep("change state to idle", () => queueScreen!.SetState(MatchmakingQueueScreen.MatchmakingScreenState.Idle));

            AddStep("change state to queueing", () => queueScreen!.SetState(MatchmakingQueueScreen.MatchmakingScreenState.Queueing));

            AddStep("change state to found match", () => queueScreen!.SetState(MatchmakingQueueScreen.MatchmakingScreenState.PendingAccept));

            AddStep("change state to waiting for room", () => queueScreen!.SetState(MatchmakingQueueScreen.MatchmakingScreenState.AcceptedWaitingForRoom));

            AddStep("change state to in room", () => queueScreen!.SetState(MatchmakingQueueScreen.MatchmakingScreenState.InRoom));
        }
    }
}
