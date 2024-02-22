// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Solo;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [HeadlessTest]
    public partial class TestSceneSoloStatisticsWatcher : OsuTestScene
    {
        protected override bool UseOnlineAPI => false;

        private UserStatisticsWatcher watcher = null!;

        [Resolved]
        private SpectatorClient spectatorClient { get; set; } = null!;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private Action<GetUsersRequest>? handleGetUsersRequest;
        private Action<GetUserRequest>? handleGetUserRequest;

        private readonly Dictionary<(int userId, string rulesetName), UserStatistics> serverSideStatistics = new Dictionary<(int userId, string rulesetName), UserStatistics>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear server-side stats", () => serverSideStatistics.Clear());
            AddStep("set up request handling", () =>
            {
                handleGetUserRequest = null;
                handleGetUsersRequest = null;

                dummyAPI.HandleRequest = request =>
                {
                    switch (request)
                    {
                        case GetUsersRequest getUsersRequest:
                            if (handleGetUsersRequest != null)
                            {
                                handleGetUsersRequest?.Invoke(getUsersRequest);
                            }
                            else
                            {
                                int userId = getUsersRequest.UserIds.Single();
                                var response = new GetUsersResponse
                                {
                                    Users = new List<APIUser>
                                    {
                                        new APIUser
                                        {
                                            Id = userId,
                                            RulesetsStatistics = new Dictionary<string, UserStatistics>
                                            {
                                                ["osu"] = tryGetStatistics(userId, "osu"),
                                                ["taiko"] = tryGetStatistics(userId, "taiko"),
                                                ["fruits"] = tryGetStatistics(userId, "fruits"),
                                                ["mania"] = tryGetStatistics(userId, "mania"),
                                            }
                                        }
                                    }
                                };
                                getUsersRequest.TriggerSuccess(response);
                            }

                            return true;

                        case GetUserRequest getUserRequest:
                            if (handleGetUserRequest != null)
                            {
                                handleGetUserRequest.Invoke(getUserRequest);
                            }
                            else
                            {
                                int userId = int.Parse(getUserRequest.Lookup);
                                string rulesetName = getUserRequest.Ruleset!.ShortName;
                                var response = new APIUser
                                {
                                    Id = userId,
                                    Statistics = tryGetStatistics(userId, rulesetName)
                                };
                                getUserRequest.TriggerSuccess(response);
                            }

                            return true;

                        default:
                            return false;
                    }
                };
            });

            AddStep("create watcher", () =>
            {
                Child = watcher = new UserStatisticsWatcher();
            });
        }

        private UserStatistics tryGetStatistics(int userId, string rulesetName)
            => serverSideStatistics.TryGetValue((userId, rulesetName), out var stats) ? stats : new UserStatistics();

        [Test]
        public void TestStatisticsUpdateFiredAfterRegistrationAddedAndScoreProcessed()
        {
            int userId = getUserId();
            long scoreId = getScoreId();
            setUpUser(userId);

            var ruleset = new OsuRuleset().RulesetInfo;

            UserStatisticsUpdate? update = null;
            registerForUpdates(scoreId, ruleset, receivedUpdate => update = receivedUpdate);

            feignScoreProcessing(userId, ruleset, 5_000_000);

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, scoreId));
            AddUntilStep("update received", () => update != null);
            AddAssert("values before are correct", () => update!.Before.TotalScore, () => Is.EqualTo(4_000_000));
            AddAssert("values after are correct", () => update!.After.TotalScore, () => Is.EqualTo(5_000_000));
        }

        [Test]
        public void TestStatisticsUpdateFiredAfterScoreProcessedAndRegistrationAdded()
        {
            int userId = getUserId();
            setUpUser(userId);

            long scoreId = getScoreId();
            var ruleset = new OsuRuleset().RulesetInfo;

            // note ordering - in this test processing completes *before* the registration is added.
            feignScoreProcessing(userId, ruleset, 5_000_000);

            UserStatisticsUpdate? update = null;
            registerForUpdates(scoreId, ruleset, receivedUpdate => update = receivedUpdate);

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, scoreId));
            AddUntilStep("update received", () => update != null);
            AddAssert("values before are correct", () => update!.Before.TotalScore, () => Is.EqualTo(4_000_000));
            AddAssert("values after are correct", () => update!.After.TotalScore, () => Is.EqualTo(5_000_000));
        }

        [Test]
        public void TestStatisticsUpdateNotFiredIfUserLoggedOut()
        {
            int userId = getUserId();
            setUpUser(userId);

            long scoreId = getScoreId();
            var ruleset = new OsuRuleset().RulesetInfo;

            UserStatisticsUpdate? update = null;
            registerForUpdates(scoreId, ruleset, receivedUpdate => update = receivedUpdate);

            feignScoreProcessing(userId, ruleset, 5_000_000);

            AddStep("log out user", () => dummyAPI.Logout());

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, scoreId));
            AddWaitStep("wait a bit", 5);
            AddAssert("update not received", () => update == null);

            AddStep("log in user", () =>
            {
                dummyAPI.Login("user", "password");
                dummyAPI.AuthenticateSecondFactor("abcdefgh");
            });
        }

        [Test]
        public void TestStatisticsUpdateNotFiredIfAnotherUserLoggedIn()
        {
            int userId = getUserId();
            setUpUser(userId);

            long scoreId = getScoreId();
            var ruleset = new OsuRuleset().RulesetInfo;

            UserStatisticsUpdate? update = null;
            registerForUpdates(scoreId, ruleset, receivedUpdate => update = receivedUpdate);

            feignScoreProcessing(userId, ruleset, 5_000_000);

            AddStep("change user", () => dummyAPI.LocalUser.Value = new APIUser { Id = getUserId() });

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, scoreId));
            AddWaitStep("wait a bit", 5);
            AddAssert("update not received", () => update == null);
        }

        [Test]
        public void TestStatisticsUpdateNotFiredIfScoreIdDoesNotMatch()
        {
            int userId = getUserId();
            setUpUser(userId);

            long scoreId = getScoreId();
            var ruleset = new OsuRuleset().RulesetInfo;

            UserStatisticsUpdate? update = null;
            registerForUpdates(scoreId, ruleset, receivedUpdate => update = receivedUpdate);

            feignScoreProcessing(userId, ruleset, 5_000_000);

            AddStep("signal another score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, getScoreId()));
            AddWaitStep("wait a bit", 5);
            AddAssert("update not received", () => update == null);
        }

        // the behaviour exercised in this test may not be final, it is mostly assumed for simplicity.
        // in the long run we may want each score's update to be entirely isolated from others, rather than have prior unobserved updates merge into the latest.
        [Test]
        public void TestIgnoredScoreUpdateIsMergedIntoNextOne()
        {
            int userId = getUserId();
            setUpUser(userId);

            long firstScoreId = getScoreId();
            var ruleset = new OsuRuleset().RulesetInfo;

            feignScoreProcessing(userId, ruleset, 5_000_000);

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, firstScoreId));

            long secondScoreId = getScoreId();

            feignScoreProcessing(userId, ruleset, 6_000_000);

            UserStatisticsUpdate? update = null;
            registerForUpdates(secondScoreId, ruleset, receivedUpdate => update = receivedUpdate);

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, secondScoreId));
            AddUntilStep("update received", () => update != null);
            AddAssert("values before are correct", () => update!.Before.TotalScore, () => Is.EqualTo(4_000_000));
            AddAssert("values after are correct", () => update!.After.TotalScore, () => Is.EqualTo(6_000_000));
        }

        [Test]
        public void TestGlobalStatisticsUpdatedAfterRegistrationAddedAndScoreProcessed()
        {
            int userId = getUserId();
            long scoreId = getScoreId();
            setUpUser(userId);

            var ruleset = new OsuRuleset().RulesetInfo;

            UserStatisticsUpdate? update = null;
            registerForUpdates(scoreId, ruleset, receivedUpdate => update = receivedUpdate);

            feignScoreProcessing(userId, ruleset, 5_000_000);

            AddStep("signal score processed", () => ((ISpectatorClient)spectatorClient).UserScoreProcessed(userId, scoreId));
            AddUntilStep("update received", () => update != null);
            AddAssert("local user values are correct", () => dummyAPI.LocalUser.Value.Statistics.TotalScore, () => Is.EqualTo(5_000_000));
            AddAssert("statistics values are correct", () => dummyAPI.Statistics.Value!.TotalScore, () => Is.EqualTo(5_000_000));
        }

        private int nextUserId = 2000;
        private long nextScoreId = 50000;

        private int getUserId() => ++nextUserId;
        private long getScoreId() => ++nextScoreId;

        private void setUpUser(int userId)
        {
            AddStep("fetch initial stats", () =>
            {
                serverSideStatistics[(userId, "osu")] = new UserStatistics { TotalScore = 4_000_000 };
                serverSideStatistics[(userId, "taiko")] = new UserStatistics { TotalScore = 3_000_000 };
                serverSideStatistics[(userId, "fruits")] = new UserStatistics { TotalScore = 2_000_000 };
                serverSideStatistics[(userId, "mania")] = new UserStatistics { TotalScore = 1_000_000 };

                dummyAPI.LocalUser.Value = new APIUser { Id = userId };
            });
        }

        private void registerForUpdates(long scoreId, RulesetInfo rulesetInfo, Action<UserStatisticsUpdate> onUpdateReady) =>
            AddStep("register for updates", () =>
            {
                watcher.RegisterForStatisticsUpdateAfter(
                    new ScoreInfo(Beatmap.Value.BeatmapInfo, new OsuRuleset().RulesetInfo, new RealmUser())
                    {
                        Ruleset = rulesetInfo,
                        OnlineID = scoreId
                    });
                watcher.LatestUpdate.BindValueChanged(update =>
                {
                    if (update.NewValue?.Score.OnlineID == scoreId)
                        onUpdateReady.Invoke(update.NewValue);
                });
            });

        private void feignScoreProcessing(int userId, RulesetInfo rulesetInfo, long newTotalScore)
            => AddStep("feign score processing", () => serverSideStatistics[(userId, rulesetInfo.ShortName)] = new UserStatistics { TotalScore = newTotalScore });
    }
}
