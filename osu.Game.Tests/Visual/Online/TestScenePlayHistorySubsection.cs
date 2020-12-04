// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections.Historical;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Users;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using System;
using System.Linq;
using osu.Framework.Testing;
using osu.Framework.Graphics.Shapes;
using static osu.Game.Users.User;

namespace osu.Game.Tests.Visual.Online
{
    public class TestScenePlayHistorySubsection : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Red);

        private readonly Bindable<User> user = new Bindable<User>();
        private readonly PlayHistorySubsection section;

        public TestScenePlayHistorySubsection()
        {
            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                section = new PlayHistorySubsection(user)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            });
        }

        [Test]
        public void TestNullValues()
        {
            AddStep("Load user", () => user.Value = user_with_null_values);
            AddAssert("Section is hidden", () => section.Alpha == 0);
        }

        [Test]
        public void TestEmptyValues()
        {
            AddStep("Load user", () => user.Value = user_with_empty_values);
            AddAssert("Section is hidden", () => section.Alpha == 0);
        }

        [Test]
        public void TestOneValue()
        {
            AddStep("Load user", () => user.Value = user_with_one_value);
            AddAssert("Section is hidden", () => section.Alpha == 0);
        }

        [Test]
        public void TestTwoValues()
        {
            AddStep("Load user", () => user.Value = user_with_two_values);
            AddAssert("Section is visible", () => section.Alpha == 1);
        }

        [Test]
        public void TestConstantValues()
        {
            AddStep("Load user", () => user.Value = user_with_constant_values);
            AddAssert("Section is visible", () => section.Alpha == 1);
        }

        [Test]
        public void TestConstantZeroValues()
        {
            AddStep("Load user", () => user.Value = user_with_zero_values);
            AddAssert("Section is visible", () => section.Alpha == 1);
        }

        [Test]
        public void TestFilledValues()
        {
            AddStep("Load user", () => user.Value = user_with_filled_values);
            AddAssert("Section is visible", () => section.Alpha == 1);
            AddAssert("Array length is the same", () => user_with_filled_values.MonthlyPlaycounts.Length == getChartValuesLength());
        }

        [Test]
        public void TestMissingValues()
        {
            AddStep("Load user", () => user.Value = user_with_missing_values);
            AddAssert("Section is visible", () => section.Alpha == 1);
            AddAssert("Array length is 7", () => getChartValuesLength() == 7);
        }

        private int getChartValuesLength() => this.ChildrenOfType<ProfileLineChart>().Single().Values.Length;

        private static readonly User user_with_null_values = new User
        {
            Id = 1
        };

        private static readonly User user_with_empty_values = new User
        {
            Id = 2,
            MonthlyPlaycounts = Array.Empty<UserHistoryCount>()
        };

        private static readonly User user_with_one_value = new User
        {
            Id = 3,
            MonthlyPlaycounts = new[]
            {
                new UserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 100 }
            }
        };

        private static readonly User user_with_two_values = new User
        {
            Id = 4,
            MonthlyPlaycounts = new[]
            {
                new UserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 1 },
                new UserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 2 }
            }
        };

        private static readonly User user_with_constant_values = new User
        {
            Id = 5,
            MonthlyPlaycounts = new[]
            {
                new UserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 5 },
                new UserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 5 },
                new UserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 5 }
            }
        };

        private static readonly User user_with_zero_values = new User
        {
            Id = 6,
            MonthlyPlaycounts = new[]
            {
                new UserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 0 },
                new UserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 0 },
                new UserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 0 }
            }
        };

        private static readonly User user_with_filled_values = new User
        {
            Id = 7,
            MonthlyPlaycounts = new[]
            {
                new UserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 1000 },
                new UserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 20 },
                new UserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 20000 },
                new UserHistoryCount { Date = new DateTime(2010, 8, 1), Count = 30 },
                new UserHistoryCount { Date = new DateTime(2010, 9, 1), Count = 50 },
                new UserHistoryCount { Date = new DateTime(2010, 10, 1), Count = 2000 },
                new UserHistoryCount { Date = new DateTime(2010, 11, 1), Count = 2100 }
            }
        };

        private static readonly User user_with_missing_values = new User
        {
            Id = 8,
            MonthlyPlaycounts = new[]
            {
                new UserHistoryCount { Date = new DateTime(2020, 1, 1), Count = 100 },
                new UserHistoryCount { Date = new DateTime(2020, 7, 1), Count = 200 }
            }
        };
    }
}
