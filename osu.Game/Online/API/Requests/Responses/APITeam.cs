// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APITeam
    {
        [JsonProperty(@"id")]
        public int Id { get; set; } = 1;

        [JsonProperty(@"name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(@"short_name")]
        public string ShortName { get; set; } = string.Empty;

        [JsonProperty(@"flag_url")]
        public string FlagUrl = string.Empty;
    }
}
