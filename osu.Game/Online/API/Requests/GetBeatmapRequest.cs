// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        private readonly BeatmapInfo beatmap;

        public GetBeatmapRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        protected override string Target => $@"beatmaps/lookup?id={beatmap.OnlineBeatmapID}&checksum={beatmap.MD5Hash}&filename={System.Uri.EscapeUriString(beatmap.Path ?? string.Empty)}";
    }
}
