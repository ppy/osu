// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2.Users;
using osu.Game.Overlays;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneUserCard : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(UserCard),
            typeof(UserGridCard),
            typeof(UserListCard)
        };

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneUserCard()
        {
            Add(new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new UserGridCard(new User
                            {
                                Username = @"flyte",
                                Id = 3103765,
                                Country = new Country { FlagName = @"JP" },
                                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                                IsOnline = true,
                                IsSupporter = true,
                                SupportLevel = 3,
                            }),
                            new UserGridCard(new User
                            {
                                Username = @"Evast",
                                Id = 8195163,
                                Country = new Country { FlagName = @"BY" },
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                                IsOnline = false,
                                LastVisit = DateTimeOffset.Now
                            })
                        }
                    },
                    new UserListCard(new User
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                        IsSupporter = true,
                        SupportLevel = 3,
                        IsOnline = false,
                        LastVisit = DateTimeOffset.Now
                    }),
                    new UserListCard(new User
                    {
                        Username = @"chocomint",
                        Id = 124493,
                        Country = new Country { FlagName = @"KR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                        IsOnline = true,
                    }),
                }
            });
        }
    }
}
