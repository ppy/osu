// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A tournament player, containing simple information about the player.
    /// </summary>
    [Serializable]
    public class TournamentPlayer : IUser
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The player's country.
        /// </summary>
        public Country? Country { get; set; }

        /// <summary>
        /// The player's global rank, or null if not available.
        /// </summary>
        public int? Rank { get; set; }

        /// <summary>
        /// A URL to the player's profile cover.
        /// </summary>
        public string CoverUrl { get; set; } = string.Empty;

        public APIUser ToUser()
        {
            var user = new APIUser
            {
                Id = Id,
                Username = Username,
                Country = Country,
                CoverUrl = CoverUrl,
            };

            user.Statistics = new UserStatistics
            {
                User = user,
                GlobalRank = Rank
            };

            return user;
        }

        int IHasOnlineID<int>.OnlineID => Id;

        bool IUser.IsBot => false;
    }
}
