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

        //[JsonProperty(@"country")]
        [JsonIgnore]
        public Country Country;

        //public Team Team;

        [JsonProperty(@"colour")]
        public string Colour;

        [JsonProperty(@"avatarUrl")]
        public string AvatarUrl;

        [JsonProperty(@"cover")]
        public UserCover Cover;

        public class UserCover
        {
            [JsonProperty(@"customUrl")]
            public string CustomUrl;

            [JsonProperty(@"url")]
            public string Url;

            [JsonProperty(@"id")]
            public int? Id;
        }
    }


}
