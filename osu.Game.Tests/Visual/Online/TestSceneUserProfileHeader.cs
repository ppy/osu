// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUserProfileHeader : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ProfileHeader),
            typeof(RankGraph),
            typeof(LineGraph),
            typeof(OverlayHeaderTabControl),
            typeof(CentreHeaderContainer),
            typeof(BottomHeaderContainer),
            typeof(DetailHeaderContainer),
            typeof(ProfileHeaderButton)
        };

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly ProfileHeader header;

        public TestSceneUserProfileHeader()
        {
            header = new ProfileHeader();
            Add(header);

            AddStep("Show test dummy", () => header.User.Value = TestSceneUserProfileOverlay.TEST_USER);

            AddStep("Show null dummy", () => header.User.Value = new User
            {
                Username = "Null"
            });

            AddStep("Show online dummy", () => header.User.Value = new User
            {
                Username = "IAmOnline",
                LastVisit = DateTimeOffset.Now,
                IsOnline = true,
            });

            AddStep("Show offline dummy", () => header.User.Value = new User
            {
                Username = "IAmOffline",
                LastVisit = DateTimeOffset.Now,
                IsOnline = false,
            });

            addOnlineStep("Show ppy", new User
            {
                Username = @"peppy",
                Id = 2,
                IsSupporter = true,
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            });

            addOnlineStep("Show flyte", new User
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FullName = @"Japan", FlagName = @"JP" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            });
        }

        private void addOnlineStep(string name, User fallback)
        {
            AddStep(name, () =>
            {
                if (api.IsLoggedIn)
                {
                    var request = new GetUserRequest(fallback.Id);
                    request.Success += user => header.User.Value = user;
                    api.Queue(request);
                }
                else
                    header.User.Value = fallback;
            });
        }
    }
}
