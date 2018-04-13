// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapSetRequest : APIRequest<APIResponseBeatmapSet>
    {
        private readonly int beatmapSetId;

        public GetBeatmapSetRequest(int beatmapSetId)
        {
            this.beatmapSetId = beatmapSetId;
        }

        protected override string Target => $@"beatmapsets/{beatmapSetId}";
    }
}
