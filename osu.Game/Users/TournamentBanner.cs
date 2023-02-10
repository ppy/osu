// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
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

        // TODO: remove when api returns @2x image link: https://github.com/ppy/osu-web/issues/9816
        public string Image => $@"{Path.ChangeExtension(ImageLowRes, null)}@2x{Path.GetExtension(ImageLowRes)}";
    }
}
