// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestScenePlayerPanel : MultiplayerTestScene
    {
        private PlayerPanel panel = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("add panel", () => Child = panel = new PlayerPanel(new MultiplayerRoomUser(1)
            {
                User = new APIUser
                {
                    Username = @"peppy",
                    Id = 2,
                    Colour = "99EB47",
                    CountryCode = CountryCode.AU,
                    CoverUrl = @"https://assets.ppy.sh/user-profile-covers/2/baba245ef60834b769694178f8f6d4f6166c5188c740de084656ad2b80f1eea7.jpeg",
                    Statistics = new UserStatistics { GlobalRank = null, CountryRank = null }
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
                            2, new MatchmakingUser
                            {
                                UserId = 2,
                                Placement = ++rank
                            }
                        }
                    }
                }
            }).WaitSafely());

            foreach (var layout in Enum.GetValues<PlayerPanelDisplayMode>())
            {
                AddStep($"set layout to {layout}", () => panel.DisplayMode = layout);
            }
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

        [Test]
        public void TestJump()
        {
            AddStep("jump", () => MultiplayerClient.SendUserMatchRequest(1, new MatchmakingAvatarActionRequest { Action = MatchmakingAvatarAction.Jump }).WaitSafely());
        }

        [Test]
        public void TestQuit()
        {
            AddToggleStep("toggle quit", quit => panel.HasQuit = quit);
        }
    }
}
