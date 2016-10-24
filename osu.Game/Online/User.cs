//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;

namespace osu.Game.Online
{
    public class User
    {
        [JsonProperty(@"username")]
        public string Name;

        [JsonProperty(@"profileColour")]
        public string Colour;

        [JsonProperty(@"id")]
        public int UserId;

        //public User MyUser = new User();
        [JsonConstructor]
        public User()
        {
        }
    }
}
