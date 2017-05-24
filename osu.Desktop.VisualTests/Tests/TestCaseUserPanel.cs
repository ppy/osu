// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Framework.Graphics;
using osu.Game.Users;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseUserPanel : TestCase
    {
        public override string Description => @"Panels for displaying a user's status";

        public override void Reset()
        {
            base.Reset();

            UserPanel flyte;
            UserPanel peppy;
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10f),
                Children = new[]
                {
                    flyte = new UserPanel(new User
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        Country = new Country { FlagName = @"JP" },
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/3103765/5b012e13611d5761caa7e24fecb3d3a16e1cf48fc2a3032cfd43dd444af83d82.jpeg"
                    }) { Width = 300 },
                    peppy = new UserPanel(new User
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/2/08cad88747c235a64fca5f1b770e100f120827ded1ffe3b66bfcd19c940afa65.jpeg"
                    }) { Width = 300 },
                },
            });

            flyte.Status.Value = new UserStatusOnline();
            peppy.Status.Value = new UserStatusSoloGame();

            AddStep(@"spectating", () => { flyte.Status.Value = new UserStatusSpectating(); });
            AddStep(@"multiplaying", () => { flyte.Status.Value = new UserStatusMultiplayerGame(); });
            AddStep(@"modding", () => { flyte.Status.Value = new UserStatusModding(); });
            AddStep(@"offline", () => { flyte.Status.Value = new UserStatusOffline(); });
            AddStep(@"null status", () => { flyte.Status.Value = null; });
        }
    }
}
