// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// A tournament player user, containing simple information about the player.
    /// </summary>
    [Serializable]
    public class TournamentUser : IUser
    {
        [JsonProperty(@"id")]
        public int OnlineID { get; set; }

        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The player's country.
        /// </summary>
        [JsonProperty("country_code")]
        public CountryCode CountryCode { get; set; }

        /// <summary>
        /// The player's global rank, or null if not available.
        /// </summary>
        public int? Rank { get; set; }

        /// <summary>
        /// A URL to the player's profile cover.
        /// </summary>
        public string CoverUrl { get; set; } = string.Empty;

        public APIUser ToAPIUser()
        {
            var user = new APIUser
            {
                Id = OnlineID,
                Username = Username,
                CountryCode = CountryCode,
                CoverUrl = CoverUrl,
            };

            user.Statistics = new UserStatistics
            {
                User = user,
                GlobalRank = Rank
            };

            return user;
        }

        bool IUser.IsBot => false;
    }
}
