// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
	internal class TestCaseUserPanel : TestCase
	{
		public override string Description => @"Panels for displaying a user's status";
        
        public override void Reset()
        {
            base.Reset();

            UserPanel p;
            Add(p = new UserPanel(new User
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FlagName = @"JP" },
                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/3103765/5b012e13611d5761caa7e24fecb3d3a16e1cf48fc2a3032cfd43dd444af83d82.jpeg"
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 300,
            });

            p.Status.Value = new UserStatusOnline();

            AddStep(@"spectating", () => { p.Status.Value = new UserStatusSpectating(); });
            AddStep(@"multiplaying", () => { p.Status.Value = new UserStatusMultiplayerGame(); });
            AddStep(@"modding", () => { p.Status.Value = new UserStatusModding(); });
        }
    }
}
