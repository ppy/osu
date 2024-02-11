// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
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
                OsuSpriteText text;

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
                Add(text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });

                statisticsProvider.Statistics.BindValueChanged(s =>
                {
                    text.Text = s.NewValue == null
                        ? "Statistics: (null)"
                        : $"Statistics: (total score: {s.NewValue.TotalScore:N0})";
                });

                Ruleset.Value = new OsuRuleset().RulesetInfo;
            });
        }

        [Test]
        public void TestInitialStatistics()
        {
            AddAssert("initial statistics populated", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(4_000_000));
        }

        [Test]
        public void TestRulesetChanges()
        {
            AddAssert("statistics from osu", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(4_000_000));
            AddStep("change ruleset to taiko", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddAssert("statistics from taiko", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(3_000_000));
            AddStep("change ruleset to catch", () => Ruleset.Value = new CatchRuleset().RulesetInfo);
            AddAssert("statistics from catch", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(2_000_000));
            AddStep("change ruleset to mania", () => Ruleset.Value = new ManiaRuleset().RulesetInfo);
            AddAssert("statistics from mania", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(1_000_000));
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

            AddAssert("statistics matches user 1001 from osu", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(4_000_000));

            AddStep("change ruleset to taiko", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddAssert("statistics matches user 1001 from taiko", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(3_000_000));

            AddStep("change ruleset to osu", () => Ruleset.Value = new OsuRuleset().RulesetInfo);
            setUser(1000, false);

            AddAssert("statistics matches user 1000 from osu", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(5_000_000));

            AddStep("change ruleset to osu", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);
            AddAssert("statistics matches user 1000 from taiko", () => statisticsProvider.Statistics.Value.AsNonNull().TotalScore, () => Is.EqualTo(6_000_000));
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
