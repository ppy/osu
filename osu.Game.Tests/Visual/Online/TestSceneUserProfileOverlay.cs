﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserProfileOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        private readonly TestUserProfileOverlay profile;

        public static readonly APIUser TEST_USER = new APIUser
        {
            Username = @"Somebody",
            Id = 1,
            CountryCode = CountryCode.Unknown,
            CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
            JoinDate = DateTimeOffset.Now.AddDays(-1),
            LastVisit = DateTimeOffset.Now,
            ProfileOrder = new[] { "me" },
            Statistics = new UserStatistics
            {
                IsRanked = true,
                GlobalRank = 2148,
                CountryRank = 1,
                PP = 4567.89m,
                Level = new UserStatistics.LevelInfo
                {
                    Current = 727,
                    Progress = 69,
                },
                RankHistory = new APIRankHistory
                {
                    Mode = @"osu",
                    Data = Enumerable.Range(2345, 45).Concat(Enumerable.Range(2109, 40)).ToArray()
                },
            },
            Badges = new[]
            {
                new Badge
                {
                    AwardedAt = DateTimeOffset.FromUnixTimeSeconds(1505741569),
                    Description = "Outstanding help by being a voluntary test subject.",
                    ImageUrl = "https://assets.ppy.sh/profile-badges/contributor.jpg",
                    Url = "https://osu.ppy.sh/wiki/en/People/Community_Contributors",
                },
                new Badge
                {
                    AwardedAt = DateTimeOffset.FromUnixTimeSeconds(1505741569),
                    Description = "Badge without a url.",
                    ImageUrl = "https://assets.ppy.sh/profile-badges/contributor.jpg",
                },
            },
            Title = "osu!volunteer",
            Colour = "ff0000",
            Achievements = Array.Empty<APIUserAchievement>(),
        };

        public TestSceneUserProfileOverlay()
        {
            Add(profile = new TestUserProfileOverlay());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Show offline dummy", () => profile.ShowUser(TEST_USER));

            AddStep("Show null dummy", () => profile.ShowUser(new APIUser
            {
                Username = @"Null",
                Id = 1,
            }));

            AddStep("Show ppy", () => profile.ShowUser(new APIUser
            {
                Username = @"peppy",
                Id = 2,
                IsSupporter = true,
                CountryCode = CountryCode.AU,
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            }));

            AddStep("Show flyte", () => profile.ShowUser(new APIUser
            {
                Username = @"flyte",
                Id = 3103765,
                CountryCode = CountryCode.JP,
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            }));

            AddStep("Show bancho", () => profile.ShowUser(new APIUser
            {
                Username = @"BanchoBot",
                Id = 3,
                IsBot = true,
                CountryCode = CountryCode.SH,
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c4.jpg"
            }));

            AddStep("Show ppy from username", () => profile.ShowUser(new APIUser { Username = @"peppy" }));
            AddStep("Show flyte from username", () => profile.ShowUser(new APIUser { Username = @"flyte" }));

            AddStep("Hide", profile.Hide);
            AddStep("Show without reload", profile.Show);
        }

        private class TestUserProfileOverlay : UserProfileOverlay
        {
            public new ProfileHeader Header => base.Header;
        }
    }
}
