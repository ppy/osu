// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseUserPage : TestCase
    {
        public override void Reset()
        {
            base.Reset();
            var userpage = new UserPageOverlay(new User
            {
                Username = @"peppy",
                Id = 2,
                Country = new Country { FlagName = @"AU" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg"
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 800,
                Height = 500
            };
            Add(userpage);
            AddStep("Toggle", userpage.ToggleVisibility);
        }
    }
}
