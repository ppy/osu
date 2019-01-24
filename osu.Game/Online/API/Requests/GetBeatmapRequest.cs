// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        private readonly BeatmapInfo beatmap;

        private string lookupString => beatmap.OnlineBeatmapID > 0 ? beatmap.OnlineBeatmapID.ToString() : $@"lookup?checksum={beatmap.MD5Hash}&filename={System.Uri.EscapeUriString(beatmap.Path)}";

        public GetBeatmapRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        protected override string Target => $@"beatmaps/{lookupString}";
    }
}
