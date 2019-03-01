// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsResponse
    {
        public IEnumerable<APIBeatmapSet> BeatmapSets;

        /// <summary>
        /// A collection of parameters which should be passed to the search endpoint to fetch the next page.
        /// </summary>
        [JsonProperty("cursor")]
        public dynamic CursorJson;
    }
}
