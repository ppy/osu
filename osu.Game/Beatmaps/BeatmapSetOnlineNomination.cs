// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    public struct BeatmapSetOnlineNomination
    {
        [JsonProperty(@"beatmapset_id")]
        public int BeatmapsetId { get; set; }

        [JsonProperty(@"reset")]
        public bool Reset { get; set; }

        [JsonProperty(@"rulesets")]
        public string[]? Rulesets { get; set; }

        [JsonProperty(@"user_id")]
        public int UserId { get; set; }
    }
}
