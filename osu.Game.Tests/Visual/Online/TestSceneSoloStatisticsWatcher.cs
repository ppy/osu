// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Solo;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [HeadlessTest]
    public partial class TestSceneSoloStatisticsWatcher : OsuTestScene
    {
        protected override bool UseOnlineAPI => false;

        private SoloStatisticsWatcher watcher = null!;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private Action<GetUsersRequest>? handleGetUsersRequest;
        private Action<GetUserRequest>? handleGetUserRequest;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set up request handling", () =>
            {
                handleGetUserRequest = null;
                handleGetUsersRequest = null;

                dummyAPI.HandleRequest = request =>
                {
                    switch (request)
                    {
                        case GetUsersRequest getUsersRequest:
                            handleGetUsersRequest?.Invoke(getUsersRequest);
                            return true;

                        case GetUserRequest getUserRequest:
                            handleGetUserRequest?.Invoke(getUserRequest);
                            return true;

                        default:
                            return false;
                    }
                };
            });

            AddStep("create watcher", () =>
            {
                Child = watcher = new SoloStatisticsWatcher();
            });
        }

        [Test]
        public void TestStatisticsUpdateFiredAfterRegistrationAddedAndScoreProcessed()
        {
            AddStep("fetch initial stats", () =>
            {
                handleGetUsersRequest = req => req.TriggerSuccess(createInitialUserResponse(1234));
                dummyAPI.LocalUser.Value = new APIUser { Id = 1234 };
            });

            SoloStatisticsUpdate? update = null;

            AddStep("register for updates", () => watcher.RegisterForStatisticsUpdateAfter(
                new ScoreInfo(Beatmap.Value.BeatmapInfo, new OsuRuleset().RulesetInfo, new RealmUser())
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                    OnlineID = 5678
                },
                receivedUpdate => update = receivedUpdate));

            AddStep("feign score processing",
                () => handleGetUserRequest =
                    req => req.TriggerSuccess(createIncrementalUserResponse(1234, 5_000_000)));

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(1234, 5678));
            AddUntilStep("update received", () => update != null);
            AddAssert("values before are correct", () => update?.Before.TotalScore, () => Is.EqualTo(4_000_000));
            AddAssert("values after are correct", () => update?.After.TotalScore, () => Is.EqualTo(5_000_000));
        }

        [Test]
        public void TestStatisticsUpdateFiredAfterScoreProcessedAndRegistrationAdded()
        {
            AddStep("fetch initial stats", () =>
            {
                handleGetUsersRequest = req => req.TriggerSuccess(createInitialUserResponse(1235));
                dummyAPI.LocalUser.Value = new APIUser { Id = 1235 };
            });

            AddStep("feign score processing",
                () => handleGetUserRequest =
                    req => req.TriggerSuccess(createIncrementalUserResponse(1235, 5_000_000)));
            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(1235, 5678));

            SoloStatisticsUpdate? update = null;

            // note ordering - this test checks that even if the registration is late, it will receive data.
            AddStep("register for updates", () => watcher.RegisterForStatisticsUpdateAfter(
                new ScoreInfo(Beatmap.Value.BeatmapInfo, new OsuRuleset().RulesetInfo, new RealmUser())
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                    OnlineID = 5678
                },
                receivedUpdate => update = receivedUpdate));
            AddUntilStep("update received", () => update != null);
            AddAssert("values before are correct", () => update?.Before.TotalScore, () => Is.EqualTo(4_000_000));
            AddAssert("values after are correct", () => update?.After.TotalScore, () => Is.EqualTo(5_000_000));
        }

        [Test]
        public void TestStatisticsUpdateNotFiredIfUserLoggedOut()
        {
            AddStep("fetch initial stats", () =>
            {
                handleGetUsersRequest = req => req.TriggerSuccess(createInitialUserResponse(1236));
                dummyAPI.LocalUser.Value = new APIUser { Id = 1236 };
            });

            SoloStatisticsUpdate? update = null;

            AddStep("register for updates", () => watcher.RegisterForStatisticsUpdateAfter(
                new ScoreInfo(Beatmap.Value.BeatmapInfo, new OsuRuleset().RulesetInfo, new RealmUser())
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                    OnlineID = 5678
                },
                receivedUpdate => update = receivedUpdate));

            AddStep("feign score processing",
                () => handleGetUserRequest =
                    req => req.TriggerSuccess(createIncrementalUserResponse(1236, 5_000_000)));

            AddStep("log out user", () => dummyAPI.Logout());

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(1236, 5678));
            AddWaitStep("wait a bit", 5);
            AddAssert("update not received", () => update == null);
        }

        [Test]
        public void TestStatisticsUpdateNotFiredIfAnotherUserLoggedIn()
        {
            AddStep("fetch initial stats", () =>
            {
                handleGetUsersRequest = req => req.TriggerSuccess(createInitialUserResponse(1237));
                dummyAPI.LocalUser.Value = new APIUser { Id = 1237 };
            });

            SoloStatisticsUpdate? update = null;

            AddStep("register for updates", () => watcher.RegisterForStatisticsUpdateAfter(
                new ScoreInfo(Beatmap.Value.BeatmapInfo, new OsuRuleset().RulesetInfo, new RealmUser())
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                    OnlineID = 5678
                },
                receivedUpdate => update = receivedUpdate));

            AddStep("feign score processing",
                () => handleGetUserRequest =
                    req => req.TriggerSuccess(createIncrementalUserResponse(1237, 5_000_000)));

            AddStep("log out user", () => dummyAPI.LocalUser.Value = new APIUser { Id = 5555 });

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(1237, 5678));
            AddWaitStep("wait a bit", 5);
            AddAssert("update not received", () => update == null);
        }

        [Test]
        public void TestStatisticsUpdateNotFiredIfScoreIdDoesNotMatch()
        {
            AddStep("fetch initial stats", () =>
            {
                handleGetUsersRequest = req => req.TriggerSuccess(createInitialUserResponse(1238));
                dummyAPI.LocalUser.Value = new APIUser { Id = 1238 };
            });

            SoloStatisticsUpdate? update = null;

            AddStep("register for updates", () => watcher.RegisterForStatisticsUpdateAfter(
                new ScoreInfo(Beatmap.Value.BeatmapInfo, new OsuRuleset().RulesetInfo, new RealmUser())
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                    OnlineID = 5678
                },
                receivedUpdate => update = receivedUpdate));

            AddStep("feign score processing",
                () => handleGetUserRequest =
                    req => req.TriggerSuccess(createIncrementalUserResponse(1238, 5_000_000)));

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(1238, 9012));
            AddWaitStep("wait a bit", 5);
            AddAssert("update not received", () => update == null);
        }

        private GetUsersResponse createInitialUserResponse(int userId) => new GetUsersResponse
        {
            Users = new List<APIUser>
            {
                new APIUser
                {
                    Id = userId,
                    RulesetsStatistics = new Dictionary<string, UserStatistics>
                    {
                        ["osu"] = new UserStatistics { TotalScore = 4_000_000 },
                        ["taiko"] = new UserStatistics { TotalScore = 3_000_000 },
                        ["fruits"] = new UserStatistics { TotalScore = 2_000_000 },
                        ["mania"] = new UserStatistics { TotalScore = 1_000_000 }
                    }
                }
            }
        };

        private APIUser createIncrementalUserResponse(int userId, long totalScore) => new APIUser
        {
            Id = userId,
            Statistics = new UserStatistics
            {
                TotalScore = totalScore
            }
        };
    }
}
