// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Framework.Graphics;
using osu.Game.Users;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneFriendsLayout : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(FriendsLayout),
            typeof(FriendsOnlineStatusControl),
            typeof(UserListToolbar)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private FriendsLayout layout;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = layout = new FriendsLayout()
            };
        });

        [Test]
        public void TestPopulate()
        {
            AddStep("Populate", () => layout.Users = getUsers());
        }

        private List<APIFriend> getUsers() => new List<APIFriend>
        {
            new APIFriend
            {
                Username = "flyte",
                Id = 3103765,
                IsOnline = true,
                CurrentModeRank = 1111,
                Country = new Country { FlagName = "JP" },
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            },
            new APIFriend
            {
                Username = "peppy",
                Id = 2,
                IsOnline = false,
                CurrentModeRank = 2222,
                Country = new Country { FlagName = "AU" },
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                IsSupporter = true,
                SupportLevel = 3,
            },
            new APIFriend
            {
                Username = "Evast",
                Id = 8195163,
                Country = new Country { FlagName = "BY" },
                CoverUrl = "https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                IsOnline = false,
                LastVisit = DateTimeOffset.Now
            }
        };
    }
}
