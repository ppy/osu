﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Online.API
{
    public interface IOnlineComponent
    {
        void APIStateChanged(APIAccess api, APIState state);
    }
}
