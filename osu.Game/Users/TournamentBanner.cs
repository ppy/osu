// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Users
{
    public class TournamentBanner
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("tournament_id")]
        public int TournamentId;

        [JsonProperty("image")]
        public string ImageLowRes = null!;

        [JsonProperty("image@2x")]
        public string Image = null!;
    }
}
