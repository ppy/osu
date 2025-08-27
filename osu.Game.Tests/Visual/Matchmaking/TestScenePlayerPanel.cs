// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Idle;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestScenePlayerPanel : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom()));
            WaitForJoined();

            AddStep("add panel", () => Child = new PlayerPanel(new MultiplayerRoomUser(1)
            {
                User = new APIUser
                {
                    Username = "Player 1",
                }
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestIncreasePlacement()
        {
            int rank = 0;

            AddStep("increase placement", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                Users =
                {
                    UserDictionary =
                    {
                        {
                            1, new MatchmakingUser
                            {
                                UserId = 1,
                                Placement = ++rank
                            }
                        }
                    }
                }
            }).WaitSafely());
        }

        [Test]
        public void TestIncreasePoints()
        {
            int points = 0;

            AddStep("increase points", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState
            {
                Users =
                {
                    UserDictionary =
                    {
                        {
                            1, new MatchmakingUser
                            {
                                UserId = 1,
                                Placement = 1,
                                Points = ++points
                            }
                        }
                    }
                }
            }).WaitSafely());
        }
    }
}
