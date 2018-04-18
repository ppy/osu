// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
