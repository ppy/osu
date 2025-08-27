// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle;
using osu.Game.Tests.Visual.Multiplayer;
using osuTK;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneIdleScreen : MultiplayerTestScene
    {
        private const int user_count = 8;

        private (MultiplayerRoomUser user, int score)[] userScores = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom()));
            WaitForJoined();

            AddStep("add list", () =>
            {
                userScores = Enumerable.Range(1, user_count).Select(i =>
                {
                    var user = new MultiplayerRoomUser(i)
                    {
                        User = new APIUser
                        {
                            Username = $"Player {i}"
                        }
                    };

                    return (user, 0);
                }).ToArray();

                Child = new ScreenStack(new IdleScreen())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f)
                };
            });

            AddStep("join users", () =>
            {
                foreach (var (user, _) in userScores)
                    MultiplayerClient.AddUser(user);
            });
        }

        [Test]
        public void TestRandomChanges()
        {
            AddStep("apply random changes", () =>
            {
                int[] deltas = Enumerable.Range(1, userScores.Length).ToArray();
                new Random().Shuffle(deltas);

                for (int i = 0; i < userScores.Length; i++)
                    userScores[i] = (userScores[i].user, userScores[i].score + deltas[i]);
                userScores = userScores.OrderByDescending(u => u.score).ToArray();

                MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
                {
                    Users =
                    {
                        UserDictionary = userScores.Select((tuple, i) => new MatchmakingUser
                        {
                            UserId = tuple.user.UserID,
                            Points = tuple.score,
                            Placement = i + 1
                        }).ToDictionary(s => s.UserId)
                    }
                }).WaitSafely();
            });
        }
    }
}
