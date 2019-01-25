// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapDetailsRequest : APIRequest<APIBeatmapMetrics>
    {
        private readonly BeatmapInfo beatmap;

        public GetBeatmapDetailsRequest(BeatmapInfo beatmap)
        {
            this.beatmap = beatmap;
        }

        protected override string Target => $@"beatmaps/{beatmap.OnlineBeatmapID}";
    }
}
