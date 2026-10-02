// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.RankedPlay;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneCompactDivisionBadge : OsuTestScene
    {
        public TestSceneCompactDivisionBadge()
        {
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Width = 150,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Full,
                            Spacing = new Vector2(10),
                            Children = DIVISIONS.Select(d => new CompactDivisionBadge(d)).ToArray(),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            ChildrenEnumerable = Enumerable.Range(1, 100).Where(i => i == 1 || i == 100 || i % 5 == 0).Select(i => new CompactDivisionBadge(
                                new RankedPlayDivision { Tier = Tier.Lustrous, DisplayName = "Lustrous", StartRating = 2400, EndRating = 3000 },
                                i
                            )),
                        }
                    }
                },
            };
        }

        public static readonly RankedPlayDivision[] DIVISIONS =
        [
            new RankedPlayDivision { Key = "Bronze-I", Tier = Tier.Bronze, Division = Division.I, DisplayName = "Bronze I", StartRating = 600, EndRating = 699 },
            new RankedPlayDivision { Key = "Bronze-II", Tier = Tier.Bronze, Division = Division.II, DisplayName = "Bronze II", StartRating = 700, EndRating = 799 },
            new RankedPlayDivision { Key = "Bronze-III", Tier = Tier.Bronze, Division = Division.III, DisplayName = "Bronze III", StartRating = 800, EndRating = 899 },
            new RankedPlayDivision { Key = "Silver-I", Tier = Tier.Silver, Division = Division.I, DisplayName = "Silver I", StartRating = 900, EndRating = 999 },
            new RankedPlayDivision { Key = "Silver-II", Tier = Tier.Silver, Division = Division.II, DisplayName = "Silver II", StartRating = 1000, EndRating = 1099 },
            new RankedPlayDivision { Key = "Silver-III", Tier = Tier.Silver, Division = Division.III, DisplayName = "Silver III", StartRating = 1100, EndRating = 1199 },
            new RankedPlayDivision { Key = "Gold-I", Tier = Tier.Gold, Division = Division.I, DisplayName = "Gold I", StartRating = 1200, EndRating = 1299 },
            new RankedPlayDivision { Key = "Gold-II", Tier = Tier.Gold, Division = Division.II, DisplayName = "Gold II", StartRating = 1300, EndRating = 1399 },
            new RankedPlayDivision { Key = "Gold-III", Tier = Tier.Gold, Division = Division.III, DisplayName = "Gold III", StartRating = 1400, EndRating = 1499 },
            new RankedPlayDivision { Key = "Platinum-I", Tier = Tier.Platinum, Division = Division.I, DisplayName = "Platinum I", StartRating = 1500, EndRating = 1599 },
            new RankedPlayDivision { Key = "Platinum-II", Tier = Tier.Platinum, Division = Division.II, DisplayName = "Platinum II", StartRating = 1600, EndRating = 1699 },
            new RankedPlayDivision { Key = "Platinum-III", Tier = Tier.Platinum, Division = Division.III, DisplayName = "Platinum III", StartRating = 1700, EndRating = 1799 },
            new RankedPlayDivision { Key = "Rhodium-I", Tier = Tier.Rhodium, Division = Division.I, DisplayName = "Rhodium I", StartRating = 1800, EndRating = 1899 },
            new RankedPlayDivision { Key = "Rhodium-II", Tier = Tier.Rhodium, Division = Division.II, DisplayName = "Rhodium II", StartRating = 1900, EndRating = 1999 },
            new RankedPlayDivision { Key = "Rhodium-III", Tier = Tier.Rhodium, Division = Division.III, DisplayName = "Rhodium III", StartRating = 2000, EndRating = 2099 },
            new RankedPlayDivision { Key = "Radiant-I", Tier = Tier.Radiant, Division = Division.I, DisplayName = "Radiant I", StartRating = 2100, EndRating = 2199 },
            new RankedPlayDivision { Key = "Radiant-II", Tier = Tier.Radiant, Division = Division.II, DisplayName = "Radiant II", StartRating = 2200, EndRating = 2299 },
            new RankedPlayDivision { Key = "Radiant-III", Tier = Tier.Radiant, Division = Division.III, DisplayName = "Radiant III", StartRating = 2300, EndRating = 2399 },
        ];
    }
}
