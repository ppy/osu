// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneLocalUserStatisticsProvider : OsuTestScene
    {
        private LocalUserStatisticsProvider statisticsProvider = null!;

        private readonly Dictionary<(int userId, string rulesetName), UserStatistics> serverSideStatistics = new Dictionary<(int userId, string rulesetName), UserStatistics>();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear statistics", () => serverSideStatistics.Clear());

            setUser(1000);

            AddStep("setup provider", () =>
            {
                OsuTextFlowContainer text;

                ((DummyAPIAccess)API).HandleRequest = r =>
                {
                    switch (r)
                    {
                        case GetUserRequest userRequest:
                            int userId = int.Parse(userRequest.Lookup);
                            string rulesetName = userRequest.Ruleset!.ShortName;
                            var response = new APIUser
                            {
                                Id = userId,
                                Statistics = tryGetStatistics(userId, rulesetName)
                            };

                            userRequest.TriggerSuccess(response);
                            return true;

                        default:
                            return false;
                    }
                };

                Clear();
                Add(statisticsProvider = new LocalUserStatisticsProvider());
                Add(text = new OsuTextFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                statisticsProvider.StatisticsUpdated += update =>
                {
                    text.Clear();

                    foreach (var ruleset in Dependencies.Get<RulesetStore>().AvailableRulesets)
                    {
                        text.AddText(statisticsProvider.GetStatisticsFor(ruleset) is UserStatistics statistics
                            ? $"{ruleset.Name} statistics: (total score: {statistics.TotalScore})"
                            : $"{ruleset.Name} statistics: (null)");
                        text.NewLine();
                    }

                    text.AddText($"latest update: {update.Ruleset}"
                                 + $" ({(update.OldStatistics?.TotalScore.ToString() ?? "null")} -> {update.NewStatistics.TotalScore})");
                };

                Ruleset.Value = new OsuRuleset().RulesetInfo;
            });
        }

        [Test]
        public void TestInitialStatistics()
        {
            AddAssert("osu statistics populated", () => statisticsProvider.GetStatisticsFor(new OsuRuleset().RulesetInfo)!.TotalScore, () => Is.EqualTo(4_000_000));
            AddAssert("taiko statistics populated", () => statisticsProvider.GetStatisticsFor(new TaikoRuleset().RulesetInfo)!.TotalScore, () => Is.EqualTo(3_000_000));
            AddAssert("catch statistics populated", () => statisticsProvider.GetStatisticsFor(new CatchRuleset().RulesetInfo)!.TotalScore, () => Is.EqualTo(2_000_000));
            AddAssert("mania statistics populated", () => statisticsProvider.GetStatisticsFor(new ManiaRuleset().RulesetInfo)!.TotalScore, () => Is.EqualTo(1_000_000));
        }

        [Test]
        public void TestUserChanges()
        {
            setUser(1001);

            AddStep("update statistics for user 1000", () =>
            {
                serverSideStatistics[(1000, "osu")] = new UserStatistics { TotalScore = 5_000_000 };
                serverSideStatistics[(1000, "taiko")] = new UserStatistics { TotalScore = 6_000_000 };
            });

            AddAssert("statistics matches user 1001 in osu",
                () => statisticsProvider.GetStatisticsFor(new OsuRuleset().RulesetInfo)!.TotalScore,
                () => Is.EqualTo(4_000_000));

            AddAssert("statistics matches user 1001 in taiko",
                () => statisticsProvider.GetStatisticsFor(new TaikoRuleset().RulesetInfo)!.TotalScore,
                () => Is.EqualTo(3_000_000));

            setUser(1000, false);

            AddAssert("statistics matches user 1000 in osu",
                () => statisticsProvider.GetStatisticsFor(new OsuRuleset().RulesetInfo)!.TotalScore,
                () => Is.EqualTo(5_000_000));

            AddAssert("statistics matches user 1000 in taiko",
                () => statisticsProvider.GetStatisticsFor(new TaikoRuleset().RulesetInfo)!.TotalScore,
                () => Is.EqualTo(6_000_000));
        }

        [Test]
        public void TestRefetchStatistics()
        {
            UserStatisticsUpdate? update = null;

            setUser(1001);

            AddStep("update statistics server side",
                () => serverSideStatistics[(1001, "osu")] = new UserStatistics { TotalScore = 9_000_000 });

            AddAssert("statistics match old score",
                () => statisticsProvider.GetStatisticsFor(new OsuRuleset().RulesetInfo)!.TotalScore,
                () => Is.EqualTo(4_000_000));

            AddStep("setup event", () =>
            {
                update = null;
                statisticsProvider.StatisticsUpdated -= onStatisticsUpdated;
                statisticsProvider.StatisticsUpdated += onStatisticsUpdated;
            });

            AddStep("request refetch", () => statisticsProvider.RefetchStatistics(new OsuRuleset().RulesetInfo));
            AddUntilStep("statistics update raised",
                () => update?.NewStatistics.TotalScore,
                () => Is.EqualTo(9_000_000));
            AddAssert("statistics match new score",
                () => statisticsProvider.GetStatisticsFor(new OsuRuleset().RulesetInfo)!.TotalScore,
                () => Is.EqualTo(9_000_000));

            void onStatisticsUpdated(UserStatisticsUpdate u) => update = u;
        }

        private UserStatistics tryGetStatistics(int userId, string rulesetName)
            => serverSideStatistics.TryGetValue((userId, rulesetName), out var stats) ? stats : new UserStatistics();

        private void setUser(int userId, bool generateStatistics = true)
        {
            AddStep($"set local user to {userId}", () =>
            {
                if (generateStatistics)
                {
                    serverSideStatistics[(userId, "osu")] = new UserStatistics { TotalScore = 4_000_000 };
                    serverSideStatistics[(userId, "taiko")] = new UserStatistics { TotalScore = 3_000_000 };
                    serverSideStatistics[(userId, "fruits")] = new UserStatistics { TotalScore = 2_000_000 };
                    serverSideStatistics[(userId, "mania")] = new UserStatistics { TotalScore = 1_000_000 };
                }

                ((DummyAPIAccess)API).LocalUser.Value = new APIUser { Id = userId };
            });
        }
    }
}
