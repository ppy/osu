// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Beatmaps
{
    public struct BeatmapSetOnlineAvailability
    {
        [JsonProperty(@"download_disabled")]
        public bool DownloadDisabled { get; set; }

        [JsonProperty(@"more_information")]
        public string ExternalLink { get; set; }
    }
}
