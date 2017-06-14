// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var userpage = new UserProfile(new User
            {
                Username = @"peppy",
                Id = 2,
                Country = new Country { FullName = @"Australia", FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = 50 },
                State = Visibility.Visible
            };
            Add(userpage);
        }
    }
}
