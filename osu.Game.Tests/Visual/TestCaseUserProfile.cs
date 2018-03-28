// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseUserProfile : OsuTestCase
    {
        private readonly TestUserProfileOverlay profile;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ProfileHeader),
            typeof(UserProfileOverlay),
            typeof(RankGraph),
            typeof(LineGraph),
        };

        public TestCaseUserProfile()
        {
            Add(profile = new TestUserProfileOverlay());
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
                Age = 1,
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
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            }));

            checkSupporterTag(true);

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
