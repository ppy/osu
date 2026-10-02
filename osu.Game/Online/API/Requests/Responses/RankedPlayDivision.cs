// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Online.API.Requests.Responses
{
    public class RankedPlayDivision
    {
        [JsonProperty(@"key")]
        public string Key = string.Empty;

        [JsonProperty(@"tier")]
        public Tier Tier;

        [JsonProperty(@"division")]
        public Division Division;

        [JsonProperty(@"display_name")]
        public string DisplayName = string.Empty;

        [JsonProperty(@"start_rating")]
        public int StartRating;

        [JsonProperty("@end_rating")]
        public int EndRating;

        public int Width => EndRating - StartRating + 1;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Tier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Rhodium,
        Radiant,
        Lustrous,
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Division
    {
        I = 1,
        II = 2,
        III = 3,
    }
}
