// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
