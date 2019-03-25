// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestCaseUserPanel : OsuTestCase
    {
        public TestCaseUserPanel()
        {
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
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
                    }) { Width = 300 },
                    peppy = new UserPanel(new User
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                        IsSupporter = true,
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
