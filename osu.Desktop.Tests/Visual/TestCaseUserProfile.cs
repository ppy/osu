// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseUserProfile : OsuTestCase
    {
        public override string Description => "Tests user's profile page.";

        public TestCaseUserProfile()
        {
            var profile = new UserProfileOverlay();
            Add(profile);

            AddStep("Show offline dummy", () => profile.ShowUser(new User
            {
                Username = @"Somebody",
                Id = 1,
                Country = new Country { FullName = @"Alien" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                JoinDate = DateTimeOffset.Now.AddDays(-1),
                LastVisit = DateTimeOffset.Now,
                Age = 1,
                ProfileOrder = new[] { "me" },
                CountryRank = 1,
                Statistics = new UserStatistics
                {
                    Rank = 2148,
                    PP = 4567.89m
                },
                RankHistory = new User.RankHistoryData
                {
                    Mode = @"osu",
                    Data = Enumerable.Range(2345, 45).Concat(Enumerable.Range(2109, 40)).ToArray()
                }
            }, false));
            AddStep("Show ppy", () => profile.ShowUser(new User
            {
                Username = @"peppy",
                Id = 2,
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            }));
            AddStep("Show flyte", () => profile.ShowUser(new User
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FullName = @"Japan", FlagName = @"JP" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            }));
            AddStep("Hide", profile.Hide);
            AddStep("Show without reload", profile.Show);
        }
    }
}
