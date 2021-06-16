// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Online.Rooms
{
    public class APIPlaylistBeatmap : APIBeatmap
    {
        [JsonProperty("checksum")]
        public string Checksum { get; set; }

        public override BeatmapInfo ToBeatmap(RulesetStore rulesets)
        {
            var b = base.ToBeatmap(rulesets);
            b.MD5Hash = Checksum;
            return b;
        }
    }
}
