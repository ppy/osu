﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseUserProfile : OsuTestCase
    {
        private readonly TestUserProfileOverlay profile;
        private APIAccess api;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ProfileHeader),
            typeof(UserProfileOverlay),
            typeof(RankGraph),
            typeof(LineGraph),
            typeof(BadgeContainer)
        };

        public TestCaseUserProfile()
        {
            Add(profile = new TestUserProfileOverlay());
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Show offline dummy", () => profile.ShowUser(new User
            {
                Username = @"Somebody",
                Id = 1,
                Country = new Country { FullName = @"Alien" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                JoinDate = DateTimeOffset.Now.AddDays(-1),
                LastVisit = DateTimeOffset.Now,
                ProfileOrder = new[] { "me" },
                Statistics = new UserStatistics
                {
                    Ranks = new UserStatistics.UserRanks { Global = 2148, Country = 1 },
                    PP = 4567.89m,
                },
                RankHistory = new User.RankHistoryData
                {
                    Mode = @"osu",
                    Data = Enumerable.Range(2345, 45).Concat(Enumerable.Range(2109, 40)).ToArray()
                },
                Badges = new[]
                {
                    new Badge
                    {
                        AwardedAt = DateTimeOffset.FromUnixTimeSeconds(1505741569),
                        Description = "Outstanding help by being a voluntary test subject.",
                        ImageUrl = "https://assets.ppy.sh/profile-badges/contributor.jpg"
                    }
                }
            }, false));

            checkSupporterTag(false);

            AddStep("Show null dummy", () => profile.ShowUser(new User
            {
                Username = @"Null",
                Id = 1,
            }, false));

            AddStep("Show ppy", () => profile.ShowUser(new User
            {
                Username = @"peppy",
                Id = 2,
                IsSupporter = true,
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            }, api.IsLoggedIn));

            checkSupporterTag(true);

            AddStep("Show flyte", () => profile.ShowUser(new User
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FullName = @"Japan", FlagName = @"JP" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            }, api.IsLoggedIn));

            AddStep("Hide", profile.Hide);
            AddStep("Show without reload", profile.Show);
        }

        private void checkSupporterTag(bool isSupporter)
        {
            AddUntilStep(() => profile.Header.User != null, "wait for load");
            if (isSupporter)
                AddAssert("is supporter", () => profile.Header.SupporterTag.Alpha == 1);
            else
                AddAssert("no supporter", () => profile.Header.SupporterTag.Alpha == 0);
        }

        private class TestUserProfileOverlay : UserProfileOverlay
        {
            public new ProfileHeader Header => base.Header;
        }
    }
}
