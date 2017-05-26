// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Desktop.VisualTests.Tests
{
    public class TestCaseSocial : TestCase
    {
        public override string Description => @"social browser overlay";

        public override void Reset()
        {
            base.Reset();

            SocialOverlay s = new SocialOverlay
            {
                Users = new[]
                {
                    new User
                    {
                        Username = @"flyte",
                        Id = 3103765,
                        Country = new Country { FlagName = @"JP" },
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/3103765/5b012e13611d5761caa7e24fecb3d3a16e1cf48fc2a3032cfd43dd444af83d82.jpeg",
                    },
                    new User
                    {
                        Username = @"Cookiezi",
                        Id = 124493,
                        Country = new Country { FlagName = @"KR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c6.jpg",
                    },
                    new User
                    {
                        Username = @"Angelism",
                        Id = 1777162,
                        Country = new Country { FlagName = @"KR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    },
                    new User
                    {
                        Username = @"Rafis",
                        Id = 2558286,
                        Country = new Country { FlagName = @"PL" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c5.jpg",
                    },
                    new User
                    {
                        Username = @"hvick225",
                        Id = 50265,
                        Country = new Country { FlagName = @"TW" },
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/50265/cb79df0d6ddd04b57d057623417aa55c505810d8e73b1a96d6e665e0e18e5770.jpeg",
                    },
                    new User
                    {
                        Username = @"peppy",
                        Id = 2,
                        Country = new Country { FlagName = @"AU" },
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/2/615362d26dc37cc4d46e61a08a2537e7cdf0e0e00f40574b18bf90156ad0280f.jpeg"
                    },
                    new User
                    {
                        Username = @"filsdelama",
                        Id = 2831793,
                        Country = new Country { FlagName = @"FR" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg"
                    },
                    new User
                    {
                        Username = @"_index",
                        Id = 652457,
                        Country = new Country { FlagName = @"RU" },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c7.jpg"
                    },
                },
            };
            Add(s);

            AddStep(@"toggle", s.ToggleVisibility);
        }
    }
}
