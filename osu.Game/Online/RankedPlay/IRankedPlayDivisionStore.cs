// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.RankedPlay
{
    /// <summary>
    /// A store which contains the ranked play rating divisions currently used by the system.
    /// </summary>
    public interface IRankedPlayDivisionStore
    {
        /// <summary>
        /// Gets a list of all divisions currently used by the system.
        /// </summary>
        Task<APIRankedPlayDivision[]> GetDivisions();

        /// <summary>
        /// Gets the division the provided rating belongs to.
        /// </summary>
        /// <returns>
        /// A <see cref="APIRankedPlayDivision"/> that contains the provided rating
        /// within its bounds. <c>null</c> otherwise.
        /// </returns>
        Task<APIRankedPlayDivision?> GetDivisionForRating(int rating);
    }
}
