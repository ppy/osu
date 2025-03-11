// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFriendsOnlineStatusControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [SetUp]
        public void SetUp() => Schedule(() => Child = new FriendOnlineStreamControl
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            CountAll = { Value = 15 },
            CountOnline = { Value = 10 },
            CountOffline = { Value = 5 }
        });
    }
}
