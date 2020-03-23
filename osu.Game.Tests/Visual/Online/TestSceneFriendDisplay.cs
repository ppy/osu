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
using osu.Game.Online.API.Requests;
using osu.Game.Online.API;
using System.Threading;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneFriendDisplay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(FriendDisplay),
            typeof(FriendOnlineStreamControl),
            typeof(UserListToolbar)
        };

        protected override bool UseOnlineAPI => true;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private GetFriendsRequest request;
        private CancellationTokenSource cancellationToken;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            request?.Cancel();
            cancellationToken?.Cancel();
            Clear();
        });

        [Test]
        public void TestOffline()
        {
            AddStep("local data", () => createDisplay(getUsers()));
        }

        [Test]
        public void TestOnline()
        {
            AddStep("online data", () =>
            {
                if (!api?.IsLoggedIn ?? false)
                    return;

                request = new GetFriendsRequest();
                request.Success += response => Schedule(() => createDisplay(response));
                api.Queue(request);
            });
        }

        private void createDisplay(List<User> users)
        {
            LoadComponentAsync(new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FriendDisplay(users)
            }, Add, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private List<User> getUsers() => new List<User>
        {
            new User
            {
                Username = "flyte",
                Id = 3103765,
                IsOnline = true,
                CurrentModeRank = 1111,
                Country = new Country { FlagName = "JP" },
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            },
            new User
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
            new User
            {
                Username = "Evast",
                Id = 8195163,
                Country = new Country { FlagName = "BY" },
                CoverUrl = "https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                IsOnline = false,
                LastVisit = DateTimeOffset.Now
            }
        };

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
