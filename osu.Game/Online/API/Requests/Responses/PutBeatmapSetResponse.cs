// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class PutBeatmapSetResponse
    {
        [JsonProperty("beatmapset_id")]
        public uint BeatmapSetId { get; set; }

        [JsonProperty("beatmap_ids")]
        public ICollection<uint> BeatmapIds { get; set; } = Array.Empty<uint>();

        [JsonProperty("files")]
        public ICollection<BeatmapSetFile> Files { get; set; } = Array.Empty<BeatmapSetFile>();
    }

    public struct BeatmapSetFile
    {
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("sha2_hash")]
        public string SHA2Hash { get; set; }
    }
}
