// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFriendsOnlineStatusControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private FriendOnlineStreamControl control;

        [SetUp]
        public void SetUp() => Schedule(() => Child = control = new FriendOnlineStreamControl
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });

        [Test]
        public void Populate()
        {
            AddStep("Populate", () => control.Populate(new List<APIUser>
            {
                new APIUser
                {
                    IsOnline = true
                },
                new APIUser
                {
                    IsOnline = false
                },
                new APIUser
                {
                    IsOnline = false
                }
            }));

            AddAssert("3 users", () => control.Items.FirstOrDefault(item => item.Status == OnlineStatus.All)?.Count == 3);
            AddAssert("1 online user", () => control.Items.FirstOrDefault(item => item.Status == OnlineStatus.Online)?.Count == 1);
            AddAssert("2 offline users", () => control.Items.FirstOrDefault(item => item.Status == OnlineStatus.Offline)?.Count == 2);
        }
    }
}
