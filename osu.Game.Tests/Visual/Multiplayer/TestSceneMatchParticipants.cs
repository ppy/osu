// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public class TestSceneMatchParticipants : MultiplayerTestScene
    {
        public TestSceneMatchParticipants()
        {
            Add(new Participants { RelativeSizeAxes = Axes.Both });

            AddStep(@"set max to null", () => Room.MaxParticipants.Value = null);
            AddStep(@"set users", () => Room.Participants.Value = new[]
            {
                new User
                {
                    Username = @"Feppla",
                    Id = 4271601,
                    Country = new Country { FlagName = @"SE" },
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    IsSupporter = true,
                },
                new User
                {
                    Username = @"Xilver",
                    Id = 3099689,
                    Country = new Country { FlagName = @"IL" },
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c2.jpg",
                    IsSupporter = true,
                },
                new User
                {
                    Username = @"Wucki",
                    Id = 5287410,
                    Country = new Country { FlagName = @"FI" },
                    CoverUrl = @"https://assets.ppy.sh/user-profile-covers/5287410/5cfeaa9dd41cbce038ecdc9d781396ed4b0108089170bf7f50492ef8eadeb368.jpeg",
                    IsSupporter = true,
                },
            });

            AddStep(@"set max", () => Room.MaxParticipants.Value = 10);
            AddStep(@"clear users", () => Room.Participants.Value = new User[] { });
            AddStep(@"set max to null", () => Room.MaxParticipants.Value = null);
        }
    }
}
