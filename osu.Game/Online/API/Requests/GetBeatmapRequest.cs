// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<APIBeatmap>
    {
        private readonly BeatmapInfo beatmapInfo;

        public GetBeatmapRequest(BeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;
        }

        protected override string Target => $@"beatmaps/lookup?id={beatmapInfo.OnlineBeatmapID}&checksum={beatmapInfo.MD5Hash}&filename={System.Uri.EscapeUriString(beatmapInfo.Path ?? string.Empty)}";
    }
}
