// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapSetRequest : APIRequest<APIBeatmapSet>
    {
        public readonly int ID;
        public readonly BeatmapSetLookupType Type;

        public GetBeatmapSetRequest(int id, BeatmapSetLookupType type = BeatmapSetLookupType.SetId)
        {
            ID = id;
            Type = type;
        }

        protected override string Target => Type == BeatmapSetLookupType.SetId ? $@"beatmapsets/{ID}" : $@"beatmapsets/lookup?beatmap_id={ID}";
    }

    public enum BeatmapSetLookupType
    {
        SetId,
        BeatmapId,
    }
}
