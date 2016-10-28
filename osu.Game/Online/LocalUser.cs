//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Threading;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics.Sprites;
using System.Threading.Tasks;
using osu.Framework.Graphics.Textures;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    public class LocalUser : User
    {
        protected APIAccess api;

        public LocalUser(APIAccess API)
        {
            api = API;
        }

        public void CheckUser()
        {
            MyUser req = new MyUser();
            //req.Perform(api);
            req.Success += delegate (User MyUser)
            {
                if (MyUser.UserId != UserId)
                {
                    UserId = MyUser.UserId;
                    Name = MyUser.Name;
                    Colour = MyUser.Colour;
                }
            };
            api.Queue(req);
        }
    }
}
