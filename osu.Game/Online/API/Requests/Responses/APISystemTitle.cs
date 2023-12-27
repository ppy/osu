// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json;

namespace osu.Game.Online.API.Requests.Responses
{
    public class APISystemTitle
    {
        [JsonProperty(@"image")]
        public string Image { get; set; } = string.Empty;

        [JsonProperty(@"url")]
        public string Url { get; set; } = string.Empty;
    }
}
