// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.Users
{
    public class User
    {
        [JsonProperty(@"id")]
        public long Id = 1;

        [JsonProperty(@"username")]
        public string Username;

        [JsonProperty(@"country_code")]
        public string CountryCode;

        [JsonProperty(@"country")]
        public Country Country;

        //public Team Team;

        [JsonProperty(@"profile_colour")]
        public string Colour;

        [JsonProperty(@"avatar_url")]
        public string AvatarUrl;

        [JsonProperty(@"cover_url")]
        public string CoverUrl;

        //[JsonProperty(@"cover")]
        //public UserCover Cover;

        public class UserCover
        {
            [JsonProperty(@"custom_url")]
            public string CustomUrl;

            [JsonProperty(@"url")]
            public string Url;

            [JsonProperty(@"id")]
            public int? Id;
        }
    }
}
