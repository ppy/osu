// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match.Results;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneResultsScreen : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("set initial results", () =>
            {
                var state = new MatchmakingRoomState
                {
                    CurrentRound = 6,
                    Stage = MatchmakingStage.Ended
                };

                int localUserId = API.LocalUser.Value.OnlineID;

                // Overall state.
                state.Users[localUserId].Placement = 1;
                state.Users[localUserId].Points = 8;
                for (int round = 1; round <= state.CurrentRound; round++)
                    state.Users[localUserId].Rounds[round].Placement = round;

                // Highest score.
                state.Users[localUserId].Rounds[1].TotalScore = 1000;

                // Highest accuracy.
                state.Users[localUserId].Rounds[2].Accuracy = 0.9995;

                // Highest combo.
                state.Users[localUserId].Rounds[3].MaxCombo = 100;

                // Most bonus score.
                state.Users[localUserId].Rounds[4].Statistics[HitResult.LargeBonus] = 50;

                // Smallest score difference.
                state.Users[localUserId].Rounds[5].TotalScore = 1000;

                // Largest score difference.
                state.Users[localUserId].Rounds[6].TotalScore = 1000;

                MultiplayerClient.ChangeMatchRoomState(state).WaitSafely();
            });

            AddStep("add results screen", () =>
            {
                Child = new ScreenStack(new SubScreenResults())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f)
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("do nothing", () => { });
        }

        [Test]
        public void TestInvalidUser()
        {
            const int invalid_user_id = 1;
            AddStep("join another user", () => MultiplayerClient.AddUser(new MultiplayerRoomUser(invalid_user_id)
            {
                User = new APIUser
                {
                    Id = invalid_user_id,
                    Username = "Invalid user"
                }
            }));

            AddStep("set results stage", () =>
            {
                var state = new MatchmakingRoomState
                {
                    CurrentRound = 6,
                    Stage = MatchmakingStage.Ended
                };

                int localUserId = API.LocalUser.Value.OnlineID;

                // Overall state.
                state.Users[localUserId].Placement = 1;
                state.Users[localUserId].Points = 8;
                state.Users[invalid_user_id].Placement = 2;
                state.Users[invalid_user_id].Points = 7;
                for (int round = 1; round <= state.CurrentRound; round++)
                    state.Users[localUserId].Rounds[round].Placement = round;

                // Highest score.
                state.Users[localUserId].Rounds[1].TotalScore = 1000;
                state.Users[invalid_user_id].Rounds[1].TotalScore = 990;

                // Highest accuracy.
                state.Users[localUserId].Rounds[2].Accuracy = 0.9995;
                state.Users[invalid_user_id].Rounds[2].Accuracy = 0.5;

                // Highest combo.
                state.Users[localUserId].Rounds[3].MaxCombo = 100;
                state.Users[invalid_user_id].Rounds[3].MaxCombo = 10;

                // Most bonus score.
                state.Users[localUserId].Rounds[4].Statistics[HitResult.LargeBonus] = 50;
                state.Users[invalid_user_id].Rounds[4].Statistics[HitResult.LargeBonus] = 25;

                // Smallest score difference.
                state.Users[localUserId].Rounds[5].TotalScore = 1000;
                state.Users[invalid_user_id].Rounds[5].TotalScore = 999;

                // Largest score difference.
                state.Users[localUserId].Rounds[6].TotalScore = 1000;
                state.Users[invalid_user_id].Rounds[6].TotalScore = 0;

                MultiplayerClient.ChangeMatchRoomState(state).WaitSafely();
            });
        }
    }
}
