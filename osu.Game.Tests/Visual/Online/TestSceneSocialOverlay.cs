// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Overlays.Social;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneSocialOverlay : OsuTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(UserPanel),
            typeof(SocialPanel),
            typeof(FilterControl),
            typeof(SocialGridPanel),
            typeof(SocialListPanel)
        };

        public TestSceneSocialOverlay()
        {
            SocialOverlay s = new SocialOverlay
            {
                Users = new[]
                {
                    new User
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        Country = new Country { FlagName = @"JP" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                    },
                    new User
                    {
                        Username = @"Cookiezi",
                        Id = 124493,
                        Country = new Country { FlagName = @"KR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    },
                    new User
                    {
                        Username = @"Angelsim",
                        Id = 1777162,
                        Country = new Country { FlagName = @"KR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                    new User
                    {
                        Username = @"Rafis",
                        Id = 2558286,
                        Country = new Country { FlagName = @"PL" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c4.jpg",
                    },
                    new User
                    {
                        Username = @"hvick225",
                        Id = 50265,
                        Country = new Country { FlagName = @"TW" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    },
                    new User
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
                    },
                    new User
                    {
                        Username = @"filsdelama",
                        Id = 2831793,
                        Country = new Country { FlagName = @"FR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c7.jpg"
                    },
                    new User
                    {
                        Username = @"_index",
                        Id = 652457,
                        Country = new Country { FlagName = @"RU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c8.jpg"
                    },
                },
            };
            Add(s);

            AddStep(@"toggle", s.ToggleVisibility);
        }
    }
}
