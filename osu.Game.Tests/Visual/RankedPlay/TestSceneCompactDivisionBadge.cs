// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.RankedPlay;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
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
                                new APIRankedPlayDivision { Tier = RankingTier.Lustrous, DisplayName = "Lustrous", StartRating = 2400, EndRating = 3000 },
                                i
                            )),
                        }
                    }
                },
            };
        }

        public static readonly APIRankedPlayDivision[] DIVISIONS =
        [
            new APIRankedPlayDivision { Key = "Bronze-I", Tier = RankingTier.Bronze, Division = RankedPlayDivision.I, DisplayName = "Bronze I", StartRating = 600, EndRating = 699 },
            new APIRankedPlayDivision { Key = "Bronze-II", Tier = RankingTier.Bronze, Division = RankedPlayDivision.II, DisplayName = "Bronze II", StartRating = 700, EndRating = 799 },
            new APIRankedPlayDivision { Key = "Bronze-III", Tier = RankingTier.Bronze, Division = RankedPlayDivision.III, DisplayName = "Bronze III", StartRating = 800, EndRating = 899 },
            new APIRankedPlayDivision { Key = "Silver-I", Tier = RankingTier.Silver, Division = RankedPlayDivision.I, DisplayName = "Silver I", StartRating = 900, EndRating = 999 },
            new APIRankedPlayDivision { Key = "Silver-II", Tier = RankingTier.Silver, Division = RankedPlayDivision.II, DisplayName = "Silver II", StartRating = 1000, EndRating = 1099 },
            new APIRankedPlayDivision { Key = "Silver-III", Tier = RankingTier.Silver, Division = RankedPlayDivision.III, DisplayName = "Silver III", StartRating = 1100, EndRating = 1199 },
            new APIRankedPlayDivision { Key = "Gold-I", Tier = RankingTier.Gold, Division = RankedPlayDivision.I, DisplayName = "Gold I", StartRating = 1200, EndRating = 1299 },
            new APIRankedPlayDivision { Key = "Gold-II", Tier = RankingTier.Gold, Division = RankedPlayDivision.II, DisplayName = "Gold II", StartRating = 1300, EndRating = 1399 },
            new APIRankedPlayDivision { Key = "Gold-III", Tier = RankingTier.Gold, Division = RankedPlayDivision.III, DisplayName = "Gold III", StartRating = 1400, EndRating = 1499 },
            new APIRankedPlayDivision { Key = "Platinum-I", Tier = RankingTier.Platinum, Division = RankedPlayDivision.I, DisplayName = "Platinum I", StartRating = 1500, EndRating = 1599 },
            new APIRankedPlayDivision { Key = "Platinum-II", Tier = RankingTier.Platinum, Division = RankedPlayDivision.II, DisplayName = "Platinum II", StartRating = 1600, EndRating = 1699 },
            new APIRankedPlayDivision { Key = "Platinum-III", Tier = RankingTier.Platinum, Division = RankedPlayDivision.III, DisplayName = "Platinum III", StartRating = 1700, EndRating = 1799 },
            new APIRankedPlayDivision { Key = "Rhodium-I", Tier = RankingTier.Rhodium, Division = RankedPlayDivision.I, DisplayName = "Rhodium I", StartRating = 1800, EndRating = 1899 },
            new APIRankedPlayDivision { Key = "Rhodium-II", Tier = RankingTier.Rhodium, Division = RankedPlayDivision.II, DisplayName = "Rhodium II", StartRating = 1900, EndRating = 1999 },
            new APIRankedPlayDivision { Key = "Rhodium-III", Tier = RankingTier.Rhodium, Division = RankedPlayDivision.III, DisplayName = "Rhodium III", StartRating = 2000, EndRating = 2099 },
            new APIRankedPlayDivision { Key = "Radiant-I", Tier = RankingTier.Radiant, Division = RankedPlayDivision.I, DisplayName = "Radiant I", StartRating = 2100, EndRating = 2199 },
            new APIRankedPlayDivision { Key = "Radiant-II", Tier = RankingTier.Radiant, Division = RankedPlayDivision.II, DisplayName = "Radiant II", StartRating = 2200, EndRating = 2299 },
            new APIRankedPlayDivision { Key = "Radiant-III", Tier = RankingTier.Radiant, Division = RankedPlayDivision.III, DisplayName = "Radiant III", StartRating = 2300, EndRating = 2399 },
        ];
    }
}
