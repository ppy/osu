// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Rulesets.Osu;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneUserProfileHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Resolved]
        private OsuConfigManager configManager { get; set; } = null!;

        private ProfileHeader header = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create header", () =>
            {
                Child = new OsuScrollContainer(Direction.Vertical)
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = header = new ProfileHeader()
                };
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("Show example user", () => header.User.Value = new UserProfileData(TestSceneUserProfileOverlay.TEST_USER, new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestProfileCoverExpanded()
        {
            AddStep("Set cover to expanded", () => configManager.SetValue(OsuSetting.ProfileCoverExpanded, true));
            AddStep("Show example user", () => header.User.Value = new UserProfileData(TestSceneUserProfileOverlay.TEST_USER, new OsuRuleset().RulesetInfo));
            AddUntilStep("Cover is expanded", () => header.ChildrenOfType<UserCoverBackground>().Single().Height, () => Is.GreaterThan(0));
        }

        [Test]
        public void TestProfileCoverCollapsed()
        {
            AddStep("Set cover to collapsed", () => configManager.SetValue(OsuSetting.ProfileCoverExpanded, false));
            AddStep("Show example user", () => header.User.Value = new UserProfileData(TestSceneUserProfileOverlay.TEST_USER, new OsuRuleset().RulesetInfo));
            AddUntilStep("Cover is collapsed", () => header.ChildrenOfType<UserCoverBackground>().Single().Height, () => Is.EqualTo(0));
        }

        [Test]
        public void TestOnlineState()
        {
            AddStep("Show online user", () => header.User.Value = new UserProfileData(new APIUser
            {
                Id = 1001,
                Username = "IAmOnline",
                LastVisit = DateTimeOffset.Now,
                IsOnline = true,
            }, new OsuRuleset().RulesetInfo));

            AddStep("Show offline user", () => header.User.Value = new UserProfileData(new APIUser
            {
                Id = 1002,
                Username = "IAmOffline",
                LastVisit = DateTimeOffset.Now.AddDays(-10),
                IsOnline = false,
            }, new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestRankedState()
        {
            AddStep("Show ranked user", () => header.User.Value = new UserProfileData(new APIUser
            {
                Id = 2001,
                Username = "RankedUser",
                Groups = new[] { new APIUserGroup { Colour = "#EB47D0", ShortName = "DEV", Name = "Developers" } },
                Statistics = new UserStatistics
                {
                    IsRanked = true,
                    GlobalRank = 15000,
                    CountryRank = 1500,
                    RankHistory = new APIRankHistory
                    {
                        Mode = @"osu",
                        Data = Enumerable.Range(2345, 45).Concat(Enumerable.Range(2109, 40)).ToArray()
                    },
                }
            }, new OsuRuleset().RulesetInfo));

            AddStep("Show unranked user", () => header.User.Value = new UserProfileData(new APIUser
            {
                Id = 2002,
                Username = "UnrankedUser",
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
        public void TestPreviousUsernames()
        {
            AddStep("Show user w/ previous usernames", () => header.User.Value = new UserProfileData(new APIUser
            {
                Id = 727,
                Username = "SomeoneIndecisive",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                Groups = new[]
                {
                    new APIUserGroup { Colour = "#EB47D0", ShortName = "DEV", Name = "Developers" },
                },
                Statistics = new UserStatistics
                {
                    IsRanked = false,
                    // web will sometimes return non-empty rank history even for unranked users.
                    RankHistory = new APIRankHistory
                    {
                        Mode = @"osu",
                        Data = Enumerable.Range(2345, 85).ToArray()
                    },
                },
                PreviousUsernames = new[] { "tsrk.", "quoicoubeh", "apagnan", "epita" }
            }, new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestManyTournamentBanners()
        {
            AddStep("Show user w/ many tournament banners", () => header.User.Value = new UserProfileData(new APIUser
            {
                Id = 728,
                Username = "Certain Guy",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                Statistics = new UserStatistics
                {
                    IsRanked = false,
                    // web will sometimes return non-empty rank history even for unranked users.
                    RankHistory = new APIRankHistory
                    {
                        Mode = @"osu",
                        Data = Enumerable.Range(2345, 85).ToArray()
                    },
                },
                TournamentBanners = new[]
                {
                    new TournamentBanner
                    {
                        Id = 15329,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_HK.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_HK@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15588,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_CN.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_CN@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15589,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_PH.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_PH@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15590,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_CL.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_CL@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15591,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_JP.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_JP@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15592,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_RU.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_RU@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15593,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_KR.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_KR@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15594,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_NZ.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_NZ@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15595,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_TH.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_TH@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15596,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_TW.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_TW@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15603,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_ID.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_ID@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15604,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_KZ.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_KZ@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15605,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_AR.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_AR@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15606,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_BR.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_BR@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15607,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_PL.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_PL@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15639,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_MX.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_MX@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15640,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_AU.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_AU@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15641,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_IT.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_IT@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15642,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_UA.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_UA@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15643,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_NL.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_NL@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15644,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_FI.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_FI@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15645,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_RO.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_RO@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15646,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_SG.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_SG@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15647,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_DE.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_DE@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15648,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_ES.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_ES@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15649,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_SE.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_SE@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15650,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_CA.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_CA@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15651,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_NO.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_NO@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15652,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_GB.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_GB@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15653,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_US.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_US@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15654,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_PL.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_PL@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15655,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_FR.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_FR@2x.jpg"
                    },
                    new TournamentBanner
                    {
                        Id = 15686,
                        TournamentId = 41,
                        ImageLowRes = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_HK.jpg",
                        Image = "https://assets.ppy.sh/tournament-banners/official/owc2023/profile/supporter_HK@2x.jpg"
                    }
                }
            }, new OsuRuleset().RulesetInfo));
        }
    }
}
