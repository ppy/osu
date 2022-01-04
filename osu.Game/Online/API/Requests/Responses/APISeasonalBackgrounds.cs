// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISeasonalBackgrounds
    {
        [JsonProperty("ends_at")]
        public DateTimeOffset EndDate;

        [JsonProperty("backgrounds")]
        public List<APISeasonalBackground> Backgrounds { get; set; }
    }

    public class APISeasonalBackground
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
