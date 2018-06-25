// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
