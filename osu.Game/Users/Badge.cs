// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;

namespace osu.Game.Users
{
    public class Badge
    {
        [JsonProperty("awarded_at")]
        public DateTimeOffset AwardedAt;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("image_url")]
        public string ImageUrl;
    }
}
