// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections.Historical;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Framework.Allocation;
using System;
using System.Linq;
using osu.Framework.Testing;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestScenePlayHistorySubsection : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Red);

        private readonly Bindable<UserProfileData?> user = new Bindable<UserProfileData?>();
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
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_null_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is hidden", () => section.Alpha == 0);
        }

        [Test]
        public void TestEmptyValues()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_empty_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is hidden", () => section.Alpha == 0);
        }

        [Test]
        public void TestOneValue()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_one_value, new OsuRuleset().RulesetInfo));
            AddAssert("Section is hidden", () => section.Alpha == 0);
        }

        [Test]
        public void TestTwoValues()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_two_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is visible", () => section.Alpha == 1);
        }

        [Test]
        public void TestConstantValues()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_constant_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is visible", () => section.Alpha == 1);
        }

        [Test]
        public void TestConstantZeroValues()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_zero_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is visible", () => section.Alpha == 1);
        }

        [Test]
        public void TestFilledValues()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_filled_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is visible", () => section.Alpha == 1);
            AddAssert("Array length is the same", () => user_with_filled_values.MonthlyPlayCounts.Length == getChartValuesLength());
        }

        [Test]
        public void TestMissingValues()
        {
            AddStep("Load user", () => user.Value = new UserProfileData(user_with_missing_values, new OsuRuleset().RulesetInfo));
            AddAssert("Section is visible", () => section.Alpha == 1);
            AddAssert("Array length is 7", () => getChartValuesLength() == 7);
        }

        private int getChartValuesLength() => this.ChildrenOfType<ProfileLineChart>().Single().Values.Length;

        private static readonly APIUser user_with_null_values = new APIUser
        {
            Id = 1
        };

        private static readonly APIUser user_with_empty_values = new APIUser
        {
            Id = 2,
            MonthlyPlayCounts = Array.Empty<APIUserHistoryCount>()
        };

        private static readonly APIUser user_with_one_value = new APIUser
        {
            Id = 3,
            MonthlyPlayCounts = new[]
            {
                new APIUserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 100 }
            }
        };

        private static readonly APIUser user_with_two_values = new APIUser
        {
            Id = 4,
            MonthlyPlayCounts = new[]
            {
                new APIUserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 1 },
                new APIUserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 2 }
            }
        };

        private static readonly APIUser user_with_constant_values = new APIUser
        {
            Id = 5,
            MonthlyPlayCounts = new[]
            {
                new APIUserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 5 },
                new APIUserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 5 },
                new APIUserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 5 }
            }
        };

        private static readonly APIUser user_with_zero_values = new APIUser
        {
            Id = 6,
            MonthlyPlayCounts = new[]
            {
                new APIUserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 0 },
                new APIUserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 0 },
                new APIUserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 0 }
            }
        };

        private static readonly APIUser user_with_filled_values = new APIUser
        {
            Id = 7,
            MonthlyPlayCounts = new[]
            {
                new APIUserHistoryCount { Date = new DateTime(2010, 5, 1), Count = 1000 },
                new APIUserHistoryCount { Date = new DateTime(2010, 6, 1), Count = 20 },
                new APIUserHistoryCount { Date = new DateTime(2010, 7, 1), Count = 20000 },
                new APIUserHistoryCount { Date = new DateTime(2010, 8, 1), Count = 30 },
                new APIUserHistoryCount { Date = new DateTime(2010, 9, 1), Count = 50 },
                new APIUserHistoryCount { Date = new DateTime(2010, 10, 1), Count = 2000 },
                new APIUserHistoryCount { Date = new DateTime(2010, 11, 1), Count = 2100 }
            }
        };

        private static readonly APIUser user_with_missing_values = new APIUser
        {
            Id = 8,
            MonthlyPlayCounts = new[]
            {
                new APIUserHistoryCount { Date = new DateTime(2020, 1, 1), Count = 100 },
                new APIUserHistoryCount { Date = new DateTime(2020, 7, 1), Count = 200 }
            }
        };
    }
}
