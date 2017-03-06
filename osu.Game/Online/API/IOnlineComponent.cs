﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Online.API
{
    public interface IOnlineComponent
    {
        void APIStateChanged(APIAccess api, APIState state);
    }
}
