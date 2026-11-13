// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;

namespace osu.Game.Online.RankedPlay
{
    /// <inheritdoc cref="IRankedPlayDivisionStore" />
    /// <remarks>
    /// It's worth noting that the interfaces implemented by this class are <see cref="Task"/>s,
    /// while the divisions they return are currently hardcoded. This is intentional, as the eventual
    /// plan is to have this data be fetched from the API.
    ///
    /// The interfaces are designed in this fashion so that the implementation could be replaced
    /// with an API-fetching one without needing to refactor all the consumers.
    /// </remarks>
    public partial class RankedPlayDivisionStore : Component, IRankedPlayDivisionStore
    {
        private static readonly APIRankedPlayDivision[] divisions =
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
            new APIRankedPlayDivision { Tier = RankingTier.Lustrous, StartRating = 2400, EndRating = 3000 },
        ];

        public Task<APIRankedPlayDivision[]> GetDivisions() => Task.FromResult(divisions);

        public Task<APIRankedPlayDivision?> GetDivisionForRating(int rating) => Task.FromResult(divisions.FirstOrDefault(d => d.StartRating <= rating && d.EndRating >= rating));
    }
}
