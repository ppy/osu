// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUserProfileHeader : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly ProfileHeader header;

        public TestSceneUserProfileHeader()
        {
            header = new ProfileHeader();
            Add(header);

            AddStep("Show test dummy", () => header.User.Value = TestSceneUserProfileOverlay.TEST_USER);

            AddStep("Show null dummy", () => header.User.Value = new APIUser
            {
                Username = "Null"
            });

            AddStep("Show online dummy", () => header.User.Value = new APIUser
            {
                Username = "IAmOnline",
                LastVisit = DateTimeOffset.Now,
                IsOnline = true,
            });

            AddStep("Show offline dummy", () => header.User.Value = new APIUser
            {
                Username = "IAmOffline",
                LastVisit = DateTimeOffset.Now,
                IsOnline = false,
            });

            addOnlineStep("Show ppy", new APIUser
            {
                Username = @"peppy",
                Id = 2,
                IsSupporter = true,
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            });

            addOnlineStep("Show flyte", new APIUser
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FullName = @"Japan", FlagName = @"JP" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            });
        }

        private void addOnlineStep(string name, APIUser fallback)
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
