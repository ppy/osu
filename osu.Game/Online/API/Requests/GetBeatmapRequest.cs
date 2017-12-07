// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapRequest : APIRequest<BeatmapInfo>
    {
        private readonly int beatmapId;

        public GetBeatmapRequest(int beatmapId)
        {
            this.beatmapId = beatmapId;
        }

        protected override string Target => $@"beatmaps/{beatmapId}";
    }
}
