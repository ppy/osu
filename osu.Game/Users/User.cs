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

        public Country Country;

        public Team Team;

        [JsonProperty(@"colour")]
        public string Colour;

        public string CoverUrl = @"https://assets.ppy.sh/user-profile-covers/2/08cad88747c235a64fca5f1b770e100f120827ded1ffe3b66bfcd19c940afa65.jpeg";
    }
}
