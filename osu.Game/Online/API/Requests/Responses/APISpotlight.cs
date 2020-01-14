// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISpotlight
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("mode_specific")]
        public bool ModeSpecific;

        [JsonProperty(@"start_date")]
        public DateTimeOffset StartDate;

        [JsonProperty(@"end_date")]
        public DateTimeOffset EndDate;

        public override string ToString() => Name;
    }
}
