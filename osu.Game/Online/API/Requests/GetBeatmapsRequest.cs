// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetBeatmapsRequest : APIRequest<GetBeatmapsResponse>
    {
        public readonly IReadOnlyList<int> BeatmapIds;

        private const int max_ids_per_request = 50;

        public GetBeatmapsRequest(int[] beatmapIds)
        {
            if (beatmapIds.Length > max_ids_per_request)
                throw new ArgumentException($"{nameof(GetBeatmapsRequest)} calls only support up to {max_ids_per_request} IDs at once");

            BeatmapIds = beatmapIds;
        }

        protected override string Target => "beatmaps/?ids[]=" + string.Join("&ids[]=", BeatmapIds);
    }
}
