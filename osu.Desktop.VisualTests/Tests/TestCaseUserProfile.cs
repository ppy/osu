// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseUserProfile : TestCase
    {
        public override string Description => "Tests user's profile page.";

        public override void Reset()
        {
            base.Reset();
            var profile = new UserProfileOverlay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = 50 },
            };
            Add(profile);

            AddStep("Show ppy", () => profile.ShowUser(new User
            {
                Username = @"peppy",
                Id = 2,
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            }));
            AddStep("Show flyte", () => profile.ShowUser(new User
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FullName = @"Japan", FlagName = @"JP" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            }));
            AddStep("Hide", profile.Hide);
            AddStep("Show without reload", profile.Show);
        }
    }
}
