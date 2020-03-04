// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2.Users;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneUserCard : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(UserCard),
            typeof(UserGridCard),
        };

        public TestSceneUserCard()
        {
            Add(new UserGridCard(new User
            {
                Username = @"flyte",
                Id = 3103765,
                Country = new Country { FlagName = @"JP" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            })
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}
