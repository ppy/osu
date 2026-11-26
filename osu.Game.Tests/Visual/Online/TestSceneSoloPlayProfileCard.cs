// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneSoloPlayProfileCard : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        private readonly Bindable<UserProfileData?> userData = new Bindable<UserProfileData?>();

        private SoloPlayProfileCard profileCard = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create", () => Child = new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Child = profileCard = new SoloPlayProfileCard
                {
                    User = { BindTarget = userData },
                },
            });
        }

        [Test]
        public void TestRankedState()
        {
            AddStep("Show ranked user", () => userData.Value = new UserProfileData(new APIUser
            {
                RankHighest = new APIUser.UserRankHighest
                {
                    Rank = 10,
                    UpdatedAt = DateTimeOffset.Now,
                },
                Statistics = generateStatistics(),
            }, new OsuRuleset().RulesetInfo));

            AddStep("Show unranked user", () => userData.Value = new UserProfileData(new APIUser
            {
                Statistics = new UserStatistics
                {
                    IsRanked = false,
                    // web will sometimes return non-empty rank history even for unranked users.
                    RankHistory = new APIRankHistory
                    {
                        Mode = @"osu",
                        Data = Enumerable.Range(2345, 85).ToArray()
                    },
                }
            }, new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestTooltips()
        {
            AddStep("set osu user", () => userData.Value = new UserProfileData(TestSceneUserProfileOverlay.TEST_USER, new OsuRuleset().RulesetInfo));

            AddAssert(
                "global rank tooltip has highest rank",
                () => profileCard.ChildrenOfType<GlobalRankDisplay>().Single().ChildrenOfType<ProfileValueDisplay>().Single().Content.TooltipText.ToString(),
                () => Does.Contain("Highest rank: #1")
            );

            AddAssert(
                "country rank has no tooltip",
                () => profileCard.ChildrenOfType<ProfileValueDisplay>().Single(d => d.Title.ToString() == "Country Ranking").Content.TooltipText.ToString(),
                () => Is.Empty
            );

            AddAssert(
                "pp has no tooltip",
                () => profileCard.ChildrenOfType<ProfileValueDisplay>().Single(d => d.Title.ToString() == "pp").Content.TooltipText.ToString(),
                () => Is.Empty
            );

            AddStep("set mania user", () =>
            {
                var data = new UserProfileData(new APIUser
                {
                    RankHighest = new APIUser.UserRankHighest
                    {
                        Rank = 10,
                        UpdatedAt = DateTimeOffset.Now,
                    },
                    Statistics = generateStatistics(),
                }, new ManiaRuleset().RulesetInfo);

                data.User.Statistics.Variants =
                [
                    new UserStatistics.Variant
                    {
                        VariantType = UserStatistics.RulesetVariant.FourKey,
                        GlobalRank = 52,
                        CountryRank = 3,
                        PP = 5431,
                    },
                    new UserStatistics.Variant
                    {
                        VariantType = UserStatistics.RulesetVariant.SevenKey,
                        GlobalRank = 62,
                        CountryRank = 4,
                        PP = 4523,
                    },
                ];

                userData.Value = data;
            });

            AddAssert(
                "global rank tooltip has variant ranks",
                () => profileCard.ChildrenOfType<GlobalRankDisplay>().Single().ChildrenOfType<ProfileValueDisplay>().Single().Content.TooltipText.ToString(),
                () => Does.Contain("4K: #52\n7K: #62")
            );

            AddAssert(
                "country rank tooltip has variant ranks",
                () => profileCard.ChildrenOfType<ProfileValueDisplay>().Single(d => d.Title.ToString() == "Country Ranking").Content.TooltipText.ToString(),
                () => Does.Contain("4K: #3\n7K: #4")
            );

            AddAssert(
                "pp tooltip has variant pps",
                () => profileCard.ChildrenOfType<ProfileValueDisplay>().Single(d => d.Title.ToString() == "pp").Content.TooltipText.ToString(),
                () => Does.Contain("4K: 5,431\n7K: 4,523")
            );
        }

        private UserStatistics generateStatistics()
        {
            int rank = RNG.Next(1, 10000);

            return new UserStatistics
            {
                IsRanked = true,
                GlobalRank = rank,
                GlobalRankPercent = rank / 1500000,
                CountryRank = RNG.Next(1, rank),
                PP = (decimal?)RNG.NextDouble(1000, 5000),
                RankHistory = new APIRankHistory
                {
                    Mode = @"osu",
                    Data = Enumerable.Range(2345, 45).Concat(Enumerable.Range(2109, 40)).ToArray()
                },
                GradesCount = new UserStatistics.Grades
                {
                    A = RNG.Next(500, 1000),
                    S = RNG.Next(300, 500),
                    SPlus = RNG.Next(300, 500),
                    SS = RNG.Next(50, 200),
                    SSPlus = RNG.Next(50, 200),
                },
            };
        }
    }
}
