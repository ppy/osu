//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using osu.Game.Graphics;
using System.Threading.Tasks;

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

        public Sprite Avatar;
        public BaseGame game;

        //protected Scheduler Scheduler;

        [JsonConstructor]
        public User()
        {
        }

        public void GetAvatar()
        {
            //todo: find a more permanent solution to accessing LocalUser
            Avatar = new Avatar(this);
        }
    }
}
