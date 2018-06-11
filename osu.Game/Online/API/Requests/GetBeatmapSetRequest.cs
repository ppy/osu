// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapSetRequest : APIRequest<APIBeatmapSet>
    {
        private readonly int id;
        private readonly BeatmapSetLookupType type;

        public GetBeatmapSetRequest(int id, BeatmapSetLookupType type = BeatmapSetLookupType.SetId)
        {
            this.id = id;
            this.type = type;
        }

        protected override string Target => type == BeatmapSetLookupType.SetId ? $@"beatmapsets/{id}" : $@"beatmapsets/lookup?beatmap_id={id}";
    }

    public enum BeatmapSetLookupType
    {
        SetId,
        BeatmapId,
    }
}
