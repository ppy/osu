// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Newtonsoft.Json;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APINewsSidebar
    {
        [JsonProperty("current_year")]
        public int CurrentYear { get; set; }

        [JsonProperty("news_posts")]
        public IEnumerable<APINewsPost> NewsPosts { get; set; }

        [JsonProperty("years")]
        public int[] Years { get; set; }
    }
}
