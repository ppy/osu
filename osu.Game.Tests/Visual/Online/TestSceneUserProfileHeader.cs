// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUserProfileHeader : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        private ProfileHeader header;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create header", () => Child = header = new ProfileHeader());
        }

        [Test]
        public void TestBasic()
        {
            AddStep("Show example user", () => header.User.Value = TestSceneUserProfileOverlay.TEST_USER);
        }

        [Test]
        public void TestOnlineState()
        {
            AddStep("Show online user", () => header.User.Value = new APIUser
            {
                Id = 1001,
                Username = "IAmOnline",
                LastVisit = DateTimeOffset.Now,
                IsOnline = true,
            });

            AddStep("Show offline user", () => header.User.Value = new APIUser
            {
                Id = 1002,
                Username = "IAmOffline",
                LastVisit = DateTimeOffset.Now.AddDays(-10),
                IsOnline = false,
            });
        }

        [Test]
        public void TestRankedState()
        {
            AddStep("Show ranked user", () => header.User.Value = new APIUser
            {
                Id = 2001,
                Username = "RankedUser",
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
            });

            AddStep("Show unranked user", () => header.User.Value = new APIUser
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
            });
        }
    }
}
