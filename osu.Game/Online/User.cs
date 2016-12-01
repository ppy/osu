//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.Online
{
    public class User
    {
        [JsonProperty(@"username")]
        public string Name;

        [JsonProperty(@"id")]
        public int Id;

        [JsonProperty(@"colour")]
        public string Colour;
    }
}
