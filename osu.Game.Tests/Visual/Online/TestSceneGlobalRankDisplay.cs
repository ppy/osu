// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Tests.Visual.UserInterface;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneGlobalRankDisplay : ThemeComparisonTestScene
    {
        public TestSceneGlobalRankDisplay()
            : base(false)
        {
        }

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            RelativeSizeAxes = Axes.Both,
            Direction = FillDirection.Full,
            Padding = new MarginPadding(20),
            Spacing = new Vector2(40),
            ChildrenEnumerable = new int?[] { 64, 423, 1453, 3468, 18_367, 48_342, 178_432, 375_231, 897_783, null }.Select(createDisplay)
        };

        private GlobalRankDisplay createDisplay(int? rank) => new GlobalRankDisplay
        {
            UserStatistics =
            {
                Value = new UserStatistics
                {
                    GlobalRank = rank,
                    GlobalRankPercent = rank / 1_000_000f,
                    Variants =
                    [
                        new UserStatistics.Variant
                        {
                            VariantType = UserStatistics.RulesetVariant.FourKey,
                            GlobalRank = rank / 3,
                        },
                        new UserStatistics.Variant
                        {
                            VariantType = UserStatistics.RulesetVariant.SevenKey,
                            GlobalRank = 2 * rank / 3,
                        }
                    ]
                },
            },
            HighestRank =
            {
                Value = rank == null
                    ? null
                    : new APIUser.UserRankHighest
                    {
                        Rank = rank.Value / 2,
                        UpdatedAt = DateTimeOffset.Now.AddMonths(-3),
                    }
            }
        };
    }
}
