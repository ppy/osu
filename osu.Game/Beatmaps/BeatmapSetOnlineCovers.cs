// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    public struct BeatmapSetOnlineCovers
    {
        public string CoverLowRes { get; set; }

        [JsonProperty(@"cover@2x")]
        public string Cover { get; set; }

        public string CardLowRes { get; set; }

        [JsonProperty(@"card@2x")]
        public string Card { get; set; }

        public string ListLowRes { get; set; }

        [JsonProperty(@"list@2x")]
        public string List { get; set; }
    }
}
