// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneResultsScreen : MultiplayerTestScene
    {
        private RankedPlayScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("add other user", () => MultiplayerClient.AddUser(new MultiplayerRoomUser(2)));

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
            AddUntilStep("screen loaded", () => screen.IsLoaded);

            setupRequestHandler();
        }

        [Test]
        public void TestBasic()
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
        public void TestMultiplier()
        {
            AddStep("set results state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.Results, state =>
            {
                int losingPlayer = state.Users.Keys.First();

                state.DamageMultiplier = 2;

                foreach (var (id, userInfo) in state.Users)
                {
                    if (id == losingPlayer)
                    {
                        userInfo.DamageInfo = new RankedPlayDamageInfo
                        {
                            RawDamage = 123_456,
                            Damage = 123_456 * 2,
                            OldLife = 1_000_000,
                            NewLife = 1_000_000 - 123_456 * 2,
                        };

                        userInfo.Life = 1_000_000 - 123_456 * 2;
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
        public void TestMissingScores()
        {
            AddStep("setup request handler", () =>
            {
                Func<APIRequest, bool>? defaultRequestHandler = ((DummyAPIAccess)API).HandleRequest;

                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    switch (request)
                    {
                        case IndexPlaylistScoresRequest index:
                            index.TriggerSuccess(new IndexedMultiplayerScores());
                            return true;

                        default:
                            return defaultRequestHandler?.Invoke(request) ?? false;
                    }
                };
            });

            AddStep("set results state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.Results, state =>
            {
                int losingPlayer = state.Users.Keys.First();

                state.DamageMultiplier = 2;

                foreach (var (id, userInfo) in state.Users)
                {
                    if (id == losingPlayer)
                    {
                        userInfo.DamageInfo = new RankedPlayDamageInfo
                        {
                            RawDamage = 123_456,
                            Damage = 123_456 * 2,
                            OldLife = 1_000_000,
                            NewLife = 1_000_000 - 123_456 * 2,
                        };
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

        private void setupRequestHandler()
        {
            AddStep("setup request handler", () =>
            {
                Func<APIRequest, bool>? defaultRequestHandler = ((DummyAPIAccess)API).HandleRequest;

                ((DummyAPIAccess)API).HandleRequest = request =>
                {
                    switch (request)
                    {
                        case IndexPlaylistScoresRequest index:
                            var result = new IndexedMultiplayerScores();

                            foreach (int userId in new[] { 2, API.LocalUser.Value.OnlineID })
                            {
                                result.Scores.Add(new MultiplayerScore
                                {
                                    ID = userId,
                                    Accuracy = RNG.NextSingle(),
                                    EndedAt = DateTimeOffset.Now,
                                    Passed = true,
                                    Rank = (ScoreRank)RNG.Next((int)ScoreRank.D, (int)ScoreRank.XH),
                                    MaxCombo = RNG.Next(1000),
                                    TotalScore = userId == 2 ? 750_000 : 750_000 - 123_456,
                                    Statistics = new Dictionary<HitResult, int>
                                    {
                                        [HitResult.Miss] = 1,
                                        [HitResult.Meh] = 50,
                                        [HitResult.Ok] = 100,
                                        [HitResult.Good] = 200,
                                        [HitResult.Great] = 300,
                                        [HitResult.Perfect] = 320,
                                        [HitResult.SmallTickHit] = 50,
                                        [HitResult.SmallTickMiss] = 25,
                                        [HitResult.LargeTickHit] = 100,
                                        [HitResult.LargeTickMiss] = 50,
                                        [HitResult.SmallBonus] = 10,
                                        [HitResult.LargeBonus] = 50
                                    },
                                    MaximumStatistics = new Dictionary<HitResult, int>
                                    {
                                        [HitResult.Perfect] = 971,
                                        [HitResult.SmallTickHit] = 75,
                                        [HitResult.LargeTickHit] = 150,
                                        [HitResult.SmallBonus] = 10,
                                        [HitResult.LargeBonus] = 50,
                                    },
                                    User = new APIUser
                                    {
                                        Id = userId,
                                        Username = $"user {userId}",
                                    }
                                });
                            }

                            index.TriggerSuccess(result);
                            return true;

                        default:
                            return defaultRequestHandler?.Invoke(request) ?? false;
                    }
                };
            });
        }
    }
}
