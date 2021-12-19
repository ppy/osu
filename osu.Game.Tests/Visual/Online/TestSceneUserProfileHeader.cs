// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile;

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
    }
}
