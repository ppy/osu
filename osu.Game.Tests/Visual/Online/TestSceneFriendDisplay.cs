// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneFriendDisplay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private FriendDisplay display;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = display = new FriendDisplay()
            };
        });

        [Test]
        public void TestOffline()
        {
            AddStep("Populate with offline test users", () => display.Users = getUsers());
        }

        [Test]
        public void TestOnline()
        {
            // No need to do anything, fetch is performed automatically.
        }

        private List<APIUser> getUsers() => new List<APIUser>
        {
            new APIUser
            {
                Username = "flyte",
                Id = 3103765,
                IsOnline = true,
                Statistics = new UserStatistics { GlobalRank = 1111 },
                CountryCode = CountryCode.JP,
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            },
            new APIUser
            {
                Username = "peppy",
                Id = 2,
                IsOnline = false,
                Statistics = new UserStatistics { GlobalRank = 2222 },
                CountryCode = CountryCode.AU,
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                IsSupporter = true,
                SupportLevel = 3,
            },
            new APIUser
            {
                Username = "Evast",
                Id = 8195163,
                CountryCode = CountryCode.BY,
                CoverUrl = "https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                IsOnline = false,
                LastVisit = DateTimeOffset.Now
            }
        };
    }
}
