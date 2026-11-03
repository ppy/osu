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
                                new APIRankedPlayDivision { Tier = RankingTier.Lustrous, StartRating = 2400, EndRating = 3000 },
                                i
                            )),
                        }
                    }
                },
            };
        }

        public static readonly APIRankedPlayDivision[] DIVISIONS =
        [
            new APIRankedPlayDivision { Tier = RankingTier.Bronze, Division = RankedPlayDivision.I, StartRating = 600, EndRating = 699 },
            new APIRankedPlayDivision { Tier = RankingTier.Bronze, Division = RankedPlayDivision.II, StartRating = 700, EndRating = 799 },
            new APIRankedPlayDivision { Tier = RankingTier.Bronze, Division = RankedPlayDivision.III, StartRating = 800, EndRating = 899 },
            new APIRankedPlayDivision { Tier = RankingTier.Silver, Division = RankedPlayDivision.I, StartRating = 900, EndRating = 999 },
            new APIRankedPlayDivision { Tier = RankingTier.Silver, Division = RankedPlayDivision.II, StartRating = 1000, EndRating = 1099 },
            new APIRankedPlayDivision { Tier = RankingTier.Silver, Division = RankedPlayDivision.III, StartRating = 1100, EndRating = 1199 },
            new APIRankedPlayDivision { Tier = RankingTier.Gold, Division = RankedPlayDivision.I, StartRating = 1200, EndRating = 1299 },
            new APIRankedPlayDivision { Tier = RankingTier.Gold, Division = RankedPlayDivision.II, StartRating = 1300, EndRating = 1399 },
            new APIRankedPlayDivision { Tier = RankingTier.Gold, Division = RankedPlayDivision.III, StartRating = 1400, EndRating = 1499 },
            new APIRankedPlayDivision { Tier = RankingTier.Platinum, Division = RankedPlayDivision.I, StartRating = 1500, EndRating = 1599 },
            new APIRankedPlayDivision { Tier = RankingTier.Platinum, Division = RankedPlayDivision.II, StartRating = 1600, EndRating = 1699 },
            new APIRankedPlayDivision { Tier = RankingTier.Platinum, Division = RankedPlayDivision.III, StartRating = 1700, EndRating = 1799 },
            new APIRankedPlayDivision { Tier = RankingTier.Rhodium, Division = RankedPlayDivision.I, StartRating = 1800, EndRating = 1899 },
            new APIRankedPlayDivision { Tier = RankingTier.Rhodium, Division = RankedPlayDivision.II, StartRating = 1900, EndRating = 1999 },
            new APIRankedPlayDivision { Tier = RankingTier.Rhodium, Division = RankedPlayDivision.III, StartRating = 2000, EndRating = 2099 },
            new APIRankedPlayDivision { Tier = RankingTier.Radiant, Division = RankedPlayDivision.I, StartRating = 2100, EndRating = 2199 },
            new APIRankedPlayDivision { Tier = RankingTier.Radiant, Division = RankedPlayDivision.II, StartRating = 2200, EndRating = 2299 },
            new APIRankedPlayDivision { Tier = RankingTier.Radiant, Division = RankedPlayDivision.III, StartRating = 2300, EndRating = 2399 },
        ];
    }
}
