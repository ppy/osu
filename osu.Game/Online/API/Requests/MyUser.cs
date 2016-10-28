//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
namespace osu.Game.Online.API.Requests
{
    public class MyUser : APIRequest<User>
    {
        protected override string Target => @"me";
    }
}
