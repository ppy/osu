// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APIUserGroup
    {
        [JsonProperty(@"colour")]
        public string? Colour { get; set; }

        [JsonProperty(@"has_listing")]
        public bool HasListings { get; set; }

        [JsonProperty(@"has_playmodes")]
        public bool HasPlaymodes { get; set; }

        [JsonProperty(@"id")]
        public int Id { get; set; }

        [JsonProperty(@"identifier")]
        public string Identifier { get; set; } = null!;

        [JsonProperty(@"is_probationary")]
        public bool IsProbationary { get; set; }

        [JsonProperty(@"name")]
        public string Name { get; set; } = null!;

        [JsonProperty(@"short_name")]
        public string ShortName { get; set; } = null!;

        [JsonProperty(@"playmodes")]
        public string[]? Playmodes { get; set; }
    }
}
